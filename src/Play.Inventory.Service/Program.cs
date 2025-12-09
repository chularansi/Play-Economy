using MassTransit;
using Play.Common.Logging;
using Play.Common.MongoDB;
using Play.Common.OpenTelemetry;
using Play.Common.Settings;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Consumers;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;
using Play.Inventory.Service.Settings;
using Polly;
using Polly.Timeout;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddSeqLogging(builder.Configuration);
builder.Services.AddTracingAndMetrics(builder.Configuration);

// Create a LoggerFactory and configure logging
using var loggerFactory = LoggerFactory.Create(logBuilder =>
{
    logBuilder.AddConsole(); // Add a console logger provider
    logBuilder.SetMinimumLevel(LogLevel.Debug); // Set minimum log level
});

builder.Services
    .AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryitems")
    .AddMongoRepository<CatalogItem>("catalogitems");

//.AddMassTransitWithRabbitMQ(retryConfigurator =>
//{
//    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
//    retryConfigurator.Ignore(typeof(UnknownItemException));
//});
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    //x.AddConsumers(Assembly.GetEntryAssembly());
    x.AddConsumer<GrantItemsConsumer>();
    x.AddConsumer<SubtractItemsConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
        cfg.UseInMemoryOutbox();
        cfg.ReceiveEndpoint("inventory-grant-items", e =>
        {
            e.ConfigureConsumer<GrantItemsConsumer>(context);
        });
        cfg.ReceiveEndpoint("inventory-subtract-items", e =>
        {
            e.ConfigureConsumer<SubtractItemsConsumer>(context);
        });
        cfg.UseRetry(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });
    });

    //var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();
    //EndpointConvention.Map<InventoryItemUpdated>(new Uri(queueSettings.InventoryItemUpdatedQueueAddress));
});

AddCatalogClient(builder, loggerFactory);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.Audience = serviceSettings.ServiceName;
                    options.Authority = serviceSettings.Authority;
                    //options.Authority = "https://localhost:8080/realms/play-auth-microservice"; https:// Only for production
                    options.RequireHttpsMetadata = false; // Only for developments. Not use for production
                    options.TokenValidationParameters.RoleClaimType = "role";
                });

builder.Services.AddAuthorizationBuilder();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseCors(cors =>
    {
        cors.WithOrigins(builder.Configuration["AllowedOrigin"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(WebApplicationBuilder builder, ILoggerFactory loggerFactory)
{
    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:8000");
    })
    .AddTransientHttpErrorPolicy(policy => policy.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            // Get an ILogger instance from the factory
            var logger = loggerFactory.CreateLogger<CatalogClient>();
            logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
        }
    ))
    .AddTransientHttpErrorPolicy(policy => policy.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var logger = loggerFactory.CreateLogger<CatalogClient>();
            logger.LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
        },
        onReset: () =>
        {
            var logger = loggerFactory.CreateLogger<CatalogClient>();
            logger.LogWarning($"Closing the circuit...");
        }
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}
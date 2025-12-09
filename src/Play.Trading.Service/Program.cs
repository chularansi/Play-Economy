using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Play.Common.Logging;
using Play.Common.OpenTelemetry;
using Play.Common.Settings;
using Play.Inventory.Contracts;
using Play.Trading.Service.Data;
using Play.Trading.Service.Observers;
using Play.Trading.Service.Settings;
using Play.Trading.Service.SignalR;
using Play.Trading.Service.StateMachines;
using Play.User.Contracts;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// get credential from user secrets
//builder.Logging.AddJsonConsole(options =>
//{
//    options.JsonWriterOptions = new JsonWriterOptions { Indented = true};
//});
ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddSeqLogging(builder.Configuration);
builder.Services.AddTracingAndMetrics(builder.Configuration);
//builder.Services.AddOpenTelemetry().WithMetrics(metricsBuilder =>
//{
//    var settings =builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
//});
//builder.Services.AddOpenTelemetry()
//    .WithTracing(tracerProviderBuilder =>
//    {
//        tracerProviderBuilder
//            .AddSource(serviceSettings.ServiceName) // Define your ActivitySource name
//            .AddSource("MassTransit")
//            .SetResourceBuilder(
//                ResourceBuilder.CreateDefault()
//                    .AddService(serviceName: serviceSettings.ServiceName, serviceVersion: "1.0.0"))
//            .AddAspNetCoreInstrumentation() // Track any inbound request into our controllers via APIs
//            .AddHttpClientInstrumentation() // Track Http calls that come from our microservice to the outside
//            // Add other instrumentation as needed
//            //.AddConsoleExporter();
//            .AddOtlpExporter(options =>
//            {
//                JaegerSettings jaegerSettings = builder.Configuration.GetSection(nameof(JaegerSettings)).Get<JaegerSettings>();

//                options.Endpoint = new Uri($"http://{jaegerSettings.Host}:{jaegerSettings.Port}");
//            }); // Export traces to an OTLP collector
//    });
//builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var password = config["PostgresDBSettings:PostgresPassword"] ?? throw new InvalidOperationException("Missing configuration: PostgresDBSettings:PostgresPassword.");

PostgresDBSettings postgresDBSettings = builder.Configuration.GetSection(nameof(PostgresDBSettings)).Get<PostgresDBSettings>();
postgresDBSettings.PostgresPassword = password;

//string connectionString = builder.Configuration.GetConnectionString("DbConnection");

builder.Services.AddDbContext<TradingSagaDbContext>(options =>
{
    options.UseNpgsql(postgresDBSettings.ConnectionString);
});

builder.Services.AddDbContext<TradingDbContext>(options =>
{
    options.UseNpgsql(postgresDBSettings.ConnectionString);
});

//AddMassTransit(builder);
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    //x.AddConsumers(typeof(Program).Assembly);
    x.AddConsumers(Assembly.GetEntryAssembly());
    //x.AddConsumer<UserUpdatedConsumer>();
    //x.AddConsumer<InventoryItemUpdatedConsumer>();
    x.AddConsumeObserver<ConsumeObserver>();

    x.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.AddDbContext<DbContext, TradingSagaDbContext>();
            r.UsePostgres();
        });
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
        cfg.UseInMemoryOutbox();
        //cfg.ReceiveEndpoint("user-updated", e =>
        //{
        //    e.ConfigureConsumer<UserUpdatedConsumer>(context);
        //});
        //cfg.ReceiveEndpoint("inventory-item-updated", e =>
        //{
        //    e.ConfigureConsumer<InventoryItemUpdatedConsumer>(context);
        //});
        cfg.ConfigureEndpoints(context);
        cfg.UseInstrumentation(serviceName: serviceSettings.ServiceName);
    });

    var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();
    EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantedItemsQueueAddress));
    EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
    EndpointConvention.Map<SubtractItems>(new Uri(queueSettings?.SubtractItemsQueueAddress));
});

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services
    .AddSingleton<IUserIdProvider, UserIdProvider>()
    .AddSingleton<MessageHub>()
    .AddSignalR();

builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    const string AccessTokenParameter = "access-token";
                    const string MessageHubPath = "/messageHub";
                    options.MapInboundClaims = false;
                    options.Audience = serviceSettings.ServiceName;
                    options.Authority = serviceSettings.Authority;
                    //options.Authority = "https://localhost:8080/realms/play-auth-microservice"; https:// Only for production
                    options.RequireHttpsMetadata = false; // Only for developments. Not use for production
                    options.TokenValidationParameters.RoleClaimType = "role";
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            var scopeClaim = identity?.FindFirst("scope");

                            if (scopeClaim is null)
                            {
                                return Task.CompletedTask;
                            }

                            var scopes = scopeClaim.Value.Split(' ');
                            identity?.RemoveClaim(scopeClaim);
                            //identity?.AddClaim((Claim)scopes.Select(scope => new Claim("scope", scope)));
                            foreach (var item in scopes)
                            {
                                identity?.AddClaim(new Claim("scope", item));
                            }
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query[AccessTokenParameter];
                            var path = context.HttpContext.Request.Path;

                            if (path.StartsWithSegments(MessageHubPath) && !string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("read_access", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "trading-api-fullaccess");
    });
    options.AddPolicy("write_access", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "trading-api-fullaccess");
    });
});


var app = builder.Build();


// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseCors(cors =>
    {
        cors.WithOrigins(builder.Configuration["AllowedOrigin"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // requirement of SignalR
    });
}
app.MapPrometheusScrapingEndpoint();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/messagehub");

await app.InitializeDbAsync();

app.Run();

//static void AddMassTransit(WebApplicationBuilder builder)
//{
    //builder.Services.AddMassTransit(configure =>
    //{
        //configure.UsingPlayEconomyRabbitMq(retryConfigurator =>
        //{
        //    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
        //    retryConfigurator.Ignore(typeof(UnknownItemException));
        //});

        //configure.AddConsumers(Assembly.GetEntryAssembly());

        //configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
        //{
        //    sagaConfigurator.UseInMemoryOutbox();
        //})
        //.EntityFrameworkRepository(r =>
        //{
        //    r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // Optimistic Pessimistic
        //    r.ExistingDbContext<TradingSagaDbContext>();
        //    r.UsePostgres();
        //});



        // As soon as the Trading Microservice loads, it needs to map any command of type GrantItems
        // to the GrantedItemsQueueAddress.
        //var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();

        //EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantedItemsQueueAddress));
        //EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
    //});
//}
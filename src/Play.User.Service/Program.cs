using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Play.Common.Logging;
using Play.Common.OpenTelemetry;
using Play.Common.PostgresDB;
using Play.Common.Settings;
using Play.User.Contracts;
using Play.User.Service.Consumers;
using Play.User.Service.Data;
using Play.User.Service.Exceptions;
using Play.User.Service.Settings;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSeqLogging(builder.Configuration);
builder.Services.AddTracingAndMetrics(builder.Configuration);

// get credential from user secrets
IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var password = config["PostgresDBSettings:PostgresPassword"] ?? throw new InvalidOperationException("Missing configuration: PostgresDBSettings:PostgresPassword.");

ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
PostgresDBSettings postgresDBSettings = builder.Configuration.GetSection(nameof(PostgresDBSettings)).Get<PostgresDBSettings>();
postgresDBSettings.PostgresPassword = password;

//string connectionString = builder.Configuration.GetConnectionString("DbConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(postgresDBSettings.ConnectionString);
});
// Build a temporary IServiceProvider to resolve services
// Note: This creates a separate service provider, which can lead to issues with scoped services if not handled carefully.
ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
ApplicationDbContext appDbContext = serviceProvider.GetService<ApplicationDbContext>();
builder.Services.AddPostgresDB(appDbContext);

//builder.Services.AddMassTransitWithRabbitMQ(retryConfigurator =>
//{
//    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
//    retryConfigurator.Ignore(typeof(UnknownUserException));
//    retryConfigurator.Ignore(typeof(InsufficientFundsException));
//});

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    //x.AddConsumers(Assembly.GetEntryAssembly());
    x.AddConsumer<DebitGilConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
        cfg.UseInMemoryOutbox();
        cfg.ReceiveEndpoint("user-debit-gil", e =>
        {
            e.ConfigureConsumer<DebitGilConsumer>(context);
        });
        cfg.UseRetry(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownUserException));
            retryConfigurator.Ignore(typeof(InsufficientFundsException));
        });
    });

    //var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();
    //EndpointConvention.Map<UserUpdated>(new Uri(queueSettings.UserUpdatedQueueAddress));
});

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
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            var scopeClaim = identity?.FindFirst("scope");

                            if (scopeClaim is null)
                            {
                                return Task.CompletedTask; ;
                            }

                            var scopes = scopeClaim.Value.Split(' ');
                            identity?.RemoveClaim(scopeClaim);
                            //identity?.AddClaim((Claim)scopes.Select(scope => new Claim("scope", scope)));
                            foreach (var item in scopes)
                            {
                                identity?.AddClaim(new Claim("scope", item));
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

//builder.Services.AddAuthorizationBuilder();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("read_access", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "userinfo-api-fullaccess");
    });
    options.AddPolicy("write_access", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "userinfo-api-fullaccess");
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            var allowedOrigin = "http://localhost:3000";
            policy.WithOrigins(allowedOrigin)
                  .WithHeaders(HeaderNames.Authorization, HeaderNames.ContentType)
                  .AllowAnyMethod();
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
            .AllowAnyMethod();
    });
}

//app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using Play.Catalog.Service.Entities;
using Play.Common.Logging;
using Play.Common.MongoDB;
using Play.Common.OpenTelemetry;
using Play.Common.Settings;
using System.Reflection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddSeqLogging(builder.Configuration);
builder.Services.AddTracingAndMetrics(builder.Configuration);

builder.Services.AddMongo()
                .AddMongoRepository<Item>("items");
//.AddMassTransitWithRabbitMQ();
//.AddJwtBearerAuthentication();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(Assembly.GetEntryAssembly());
    //x.AddConsumer<GrantItemsConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
        cfg.UseInMemoryOutbox();
        //cfg.ReceiveEndpoint("inventory-grant-items", e =>
        //{
        //    e.ConfigureConsumer<GrantItemsConsumer>(context);
        //});
        cfg.UseRetry(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
        });
    });
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("read_access", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog-api-fullaccess");
    });
    options.AddPolicy("write_access", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog-api-fullaccess");
    });
});

//builder.Services.AddAuthorizationBuilder()
//    .AddFallbackPolicy("read_access", authBuilder =>
//    {
//        authBuilder.RequireClaim("scope", "catalog-api-fullaccess");
//    })
//    .AddPolicy("write_access", authBuilder =>
//    {
//        authBuilder.RequireClaim("scope", "catalog-api-fullaccess");
//        authBuilder.RequireRole("Admin");
//    });

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

//if (app.Environment.IsDevelopment())
//{
//    app.UseCors(cors => 
//    {
//        cors.WithOrigins(builder.Configuration["AllowedOrigin"])
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

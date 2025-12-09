using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Play.Common.Settings;

namespace Play.Common.Identity
{
    public class ConfigureJwtBearerOptions(IConfiguration configuration) : IConfigureNamedOptions<JwtBearerOptions>
    {
        public void Configure(string name, JwtBearerOptions options)
        {
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                var serviceSetting = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                //options.Authority = serviceSetting.Authority;
                //options.Audience = serviceSetting.ServiceName;
                options.Authority = "http://localhost:8080/realms/play-auth-microservice";
                options.Audience = "Catalog";
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "http://localhost:8080/realms/play-auth-microservice",
                    ValidAudience = "Catalog",
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }
}

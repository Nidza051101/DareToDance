using System.Text;
using DareToDance.Application.Common.Persistence;
using DareToDance.Application.Common.Security;
using DareToDance.Application.Common.Services;
using DareToDance.Application.Services.Authentication.Jwt;
using DareToDance.Application.Services.Authentication.Otp;
using DareToDance.Infrastructure.Authentication;
using DareToDance.Infrastructure.Persistence;
using DareToDance.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DareToDance.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(ConfigurationManager configuration)
        {
            services.AddAuth(configuration);
            services.AddOtp();

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<IUserRepository, UserRepository>();

            return services;
        }

        private IServiceCollection AddOtp()
        {
            services.AddOptions<OtpSettings>()
                .BindConfiguration(OtpSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IOtpCodeGenerator, OtpCodeGenerator>();
            services.AddSingleton<IOtpRepository, OtpRepository>();
            services.AddSingleton<IEmailSender, ConsoleEmailSender>();

            return services;
        }

        private IServiceCollection AddAuth(ConfigurationManager configuration)
        {
            services.AddOptions<JwtSettings>()
                .BindConfiguration(JwtSettings.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            // configured lazily through IOptions so the validated JwtSettings is the single
            // source of truth (and configuration overrides in tests are picked up)
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
                {
                    var jwtSettings = jwtOptions.Value;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        NameClaimType = JwtRegisteredClaimNames.Sub,
                        RoleClaimType = "role",
                    };
                });

            return services;
        }
    }
}

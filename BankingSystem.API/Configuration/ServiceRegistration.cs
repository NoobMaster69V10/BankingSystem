using System.Text;
using BankingSystem.API.CustomValidators;
using Microsoft.OpenApi.Models;
using BankingSystem.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BankingSystem.Domain.ConfigurationSettings.Jwt;
using BankingSystem.Domain.ConfigurationSettings.Email;
using BankingSystem.Domain.ConfigurationSettings.Seeder;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using BankingSystem.Domain.ConfigurationSettings.Encryption;

namespace BankingSystem.API.Configuration;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EncryptionSettings>(configuration.GetSection("Encryption"));
        services.Configure<SeederSettings>(configuration.GetSection("Seeder"));

        services.AddHttpClient();
        services.AddLogging();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter '{token}' in the field below."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        services.AddIdentity<IdentityPerson, IdentityRole>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(1);
                opt.Lockout.MaxFailedAccessAttempts = 3;
            })
            .AddEntityFrameworkStores<BankingSystemDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<CustomPasswordValidator<IdentityPerson>>();

        services.Configure<DataProtectionTokenProviderOptions>(
            opt => opt.TokenLifespan = TimeSpan.FromDays(30));

        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });
    }
}
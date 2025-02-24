using System.Text;
using BankingSystem.Core.Identity;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BankingSytem.API.Configure;

public static class IdentityServiceRegistration
{
    public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<IdentityPerson, IdentityRole>()
            .AddEntityFrameworkStores<BankingSystemDbContext>()
            .AddDefaultTokenProviders();

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
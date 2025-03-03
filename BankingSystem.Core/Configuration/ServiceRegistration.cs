using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities.Email;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace BankingSystem.Core.Configuration;

public static class ServiceRegistration
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IBankCardService, BankCardService>();
        services.AddScoped<IAtmService, AtmService>();
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IPersonAuthService, PersonAuthService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IBankReportService, BankReportService>();
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IHasherService, HashingService>(); 
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.Configure<FormOptions>(o => {
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = int.MaxValue;
            o.MemoryBufferThreshold = int.MaxValue;
        });
    }
}
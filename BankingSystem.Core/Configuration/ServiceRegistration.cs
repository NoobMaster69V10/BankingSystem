using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
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
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IEmailService, EmailService>();
    }
}
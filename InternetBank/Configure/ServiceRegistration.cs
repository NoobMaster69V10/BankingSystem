using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.Infrastructure.ExternalApis;
using BankingSystem.Infrastructure.Repository;
using BankingSystem.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace InternetBank.UI.Configure;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddIdentity<IdentityPerson, IdentityRole>()
            .AddEntityFrameworkStores<BankingSystemDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IPersonAuthService, PersonAuthService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankCardRepository, BankCardRepository>();
        services.AddScoped<IBankCardService, BankCardService>();
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IAccountTransactionRepository, TransactionRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IExchangeRateApi, ExchangeRateApi>();
        services.AddScoped<IAtmService, AtmService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddHttpClient();
        services.AddLogging();
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddTransient<ApplicationDataSeeder>();
    }
}
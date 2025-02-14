using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using BankingSystem.Infrastructure.Data.ExternalApis;
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
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBankCardRepository, BankCardRepository>();
        services.AddScoped<IBankCardService, BankCardService>();
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IAccountTransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IExchangeRateApi, ExchangeRateApi>();
        services.AddScoped<IAtmService, AtmService>();
        services.AddHttpClient();
    }
}
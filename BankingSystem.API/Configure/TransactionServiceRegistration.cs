using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Infrastructure.Repository;

namespace BankingSystem.API.Configure;

public static class TransactionServiceRegistration
{
    public static void AddTransactionServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IAccountTransactionRepository, TransactionRepository>();
        services.AddScoped<IAtmService, AtmService>();
    }
}
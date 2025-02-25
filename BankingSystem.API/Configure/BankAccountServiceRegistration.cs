using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Infrastructure.Repository;

namespace BankingSystem.API.Configure;

public static class BankAccountServiceRegistration
{
    public static void AddBankAccountServices(this IServiceCollection services)
    {
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
    }
}
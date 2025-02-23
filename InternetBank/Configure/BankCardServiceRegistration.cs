using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Infrastructure.Repository;

namespace InternetBank.UI.Configure;

public static class BankCardServiceRegistration
{
    public static void AddBankCardServices(this IServiceCollection services)
    {
        services.AddScoped<IBankCardRepository, BankCardRepository>();
        services.AddScoped<IBankCardService, BankCardService>();
    }
}
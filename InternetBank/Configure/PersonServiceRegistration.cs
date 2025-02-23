using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Infrastructure.Repository;

namespace InternetBank.UI.Configure;

public static class PersonServiceRegistration
{
    public static void AddPersonServices(this IServiceCollection services)
    {
        services.AddScoped<IPersonAuthService, PersonAuthService>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPersonService, PersonService>();
    }
}
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BankingSystem.Infrastructure.Repository;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.DependencyInjection;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Infrastructure.Data.DatabaseConfiguration;
using BankingSystem.Infrastructure.ExternalApis;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.Infrastructure.Data.DatabaseContext;

namespace BankingSystem.Infrastructure.Configure;

using UnitOfWork;
public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        services.AddScoped<IDbConnection>(_ =>
                new SqlConnection(connectionString));

        services.AddDbContext<BankingSystemDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IBankReportRepository, BankReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
        services.AddScoped<IBankCardRepository, BankCardRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IExchangeRateApi, ExchangeRateApi>();
        services.AddScoped<IDatabaseConfiguration, DatabaseConfiguration>();
        services.AddHostedService<DatabaseConfiguratorBackground>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddHostedService<DatabaseSeederBackground>();
    }
}
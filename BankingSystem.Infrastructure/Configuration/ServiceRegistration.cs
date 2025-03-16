using System.Data;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Infrastructure.Data.DatabaseConfiguration;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.Infrastructure.ExternalApis;
using BankingSystem.Infrastructure.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingSystem.Infrastructure.Configuration;

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
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
        services.AddScoped<IBankCardRepository, BankCardRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ICurrencyExchangeClient, CurrencyExchangeClient>();
        services.AddScoped<IDatabaseConfiguration, DatabaseConfiguration>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddHostedService<DatabaseConfiguratorBackground>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddHostedService<DatabaseSeederBackground>();
    }
}
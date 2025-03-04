using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BankingSystem.Infrastructure.Repository;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.DependencyInjection;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Infrastructure.ExternalApis;
using BankingSystem.Infrastructure.Data.DataSeeder;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using BankingSystem.Infrastructure.Data.DatabaseConfiguration;

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
        services.AddScoped<IAccountTransactionRepository, AccountTransactionRepository>();
        services.AddScoped<IBankCardRepository, BankCardRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IExchangeRateApi, ExchangeRateApi>();
        services.AddScoped<IAtmRepository, AtmRepository>();
        services.AddTransient<ApplicationDataSeeder>();
        services.AddScoped<DatabaseConfiguration>();
    }
}
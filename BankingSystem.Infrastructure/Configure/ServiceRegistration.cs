using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Infrastructure.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using BankingSystem.Infrastructure.Data.DataSeeder;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Infrastructure.Configure;

using Data.DatabaseConfiguration;
using Domain.ExternalApiContracts;
using ExternalApis;
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

        services.AddScoped<IReportRepository, ReportRepository>();
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
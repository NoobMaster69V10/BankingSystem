using Dapper;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BankingSystem.Core.ServiceContracts;
using Microsoft.Extensions.DependencyInjection;
using BankingSystem.Infrastructure.Data.DatabaseContext;

namespace BankingSystem.Infrastructure.Data.DatabaseConfiguration;

public class DatabaseConfiguration(IServiceProvider serviceProvider, IConfiguration configuration, ILoggerService loggerService)
{
    public async Task ConfigureDatabase()
    {
        //var databaseName = _configuration.GetConnectionString("DefaultConnection")!.Split("=")[2].Split(";")[0];
        var databaseName = Environment.GetEnvironmentVariable("CONNECTION_STRING")!.Split("=")[2].Split(";")[0];

        var databaseCheckQuery = $"SELECT CASE WHEN EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}') THEN 1 ELSE 0 END";
        var connection = serviceProvider.GetRequiredService<IDbConnection>();
        using (connection)
        {
            try
            {
                await connection.ExecuteAsync(databaseCheckQuery);
                loggerService.LogSuccess("Database already exists!");
            }
            catch (Exception)
            {
                var dbContext = serviceProvider.GetRequiredService<BankingSystemDbContext>();
                await dbContext.Database.MigrateAsync();

                var createBankAccountTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                    @"..\BankingSystem.SQL\Tables\BankAccounts.sql"));

                var createBankCardTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                    @"..\BankingSystem.SQL\Tables\BankCards.sql"));

                var createAccountTransactionTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                    @"..\BankingSystem.SQL\Tables\AccountTransactions.sql"));
                
                await connection.ExecuteAsync(createBankAccountTable);
                await connection.ExecuteAsync(createBankCardTable);
                await connection.ExecuteAsync(createAccountTransactionTable);
                loggerService.LogSuccess("Database created successfully!");
            }
        }
    }
}
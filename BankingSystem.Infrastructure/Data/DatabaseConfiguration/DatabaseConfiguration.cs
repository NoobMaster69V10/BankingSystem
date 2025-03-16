using Dapper;
using System.Data;
using Microsoft.EntityFrameworkCore;
using BankingSystem.Core.ServiceContracts;
using Microsoft.Extensions.DependencyInjection;
using BankingSystem.Infrastructure.Data.DatabaseContext;

namespace BankingSystem.Infrastructure.Data.DatabaseConfiguration;

public class DatabaseConfiguration : IDatabaseConfiguration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerService _loggerService;
    public DatabaseConfiguration(IServiceProvider serviceProvider, ILoggerService loggerService)
    {
        _loggerService = loggerService;
        _serviceProvider = serviceProvider;
    }
    
    public async Task ConfigureDatabaseAsync()
    {
        var databaseName = Environment.GetEnvironmentVariable("CONNECTION_STRING")!.Split("=")[2].Split(";")[0];

        var databaseCheckQuery = $"SELECT CASE WHEN EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}') THEN 1 ELSE 0 END";
        using var scope = _serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        try
        {
            var connection = scopedProvider.GetRequiredService<IDbConnection>();
            await connection.ExecuteAsync(databaseCheckQuery);

            _loggerService.LogError("Database already exists!");
        }
        catch (Exception)
        {
            var dbContext = scopedProvider.GetRequiredService<BankingSystemDbContext>();
            await dbContext.Database.MigrateAsync();

            var connection = scopedProvider.GetRequiredService<IDbConnection>();

            var createBankAccountTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\Tables\BankAccounts.sql"));

            var createBankCardTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\Tables\BankCards.sql"));

            var createAccountTransactionTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\Tables\AccountTransactions.sql"));
            var createRefreshTokenTable = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\Tables\RefreshToken.sql"));
            var createTransferBetweenAccountsProcedure = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\StoredProcedures\TransferBetweenAccounts.sql"));
            var createUpdateCardTrigger = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\Triggers\trg_UpdateCardStatusOnAccountStatusChange.sql"));
            var createPersonInfoView = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                @"..\BankingSystem.SQL\Views\vw_PersonInfo.sql"));

            await connection.ExecuteAsync(createBankAccountTable);
            await connection.ExecuteAsync(createBankCardTable);
            await connection.ExecuteAsync(createAccountTransactionTable);
            await connection.ExecuteAsync(createRefreshTokenTable);
            await connection.ExecuteAsync(createTransferBetweenAccountsProcedure);
            await connection.ExecuteAsync(createUpdateCardTrigger);
            await connection.ExecuteAsync(createPersonInfoView);

            _loggerService.LogSuccess("Database migration completed successfully!");
        }
    }
}
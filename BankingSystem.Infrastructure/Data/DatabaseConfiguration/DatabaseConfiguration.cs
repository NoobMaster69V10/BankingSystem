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

            var sqlQueryPaths = new List<string>()
            {
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Tables{Path.DirectorySeparatorChar}BankAccounts.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Tables{Path.DirectorySeparatorChar}BankCards.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Tables{Path.DirectorySeparatorChar}AccountTransactions.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Tables{Path.DirectorySeparatorChar}RefreshToken.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}StoredProcedures{Path.DirectorySeparatorChar}TransferBetweenAccounts.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Triggers{Path.DirectorySeparatorChar}trg_UpdateCardStatusOnAccountStatusChange.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Views{Path.DirectorySeparatorChar}vw_PersonInfo.sql",
                $"..{Path.DirectorySeparatorChar}BankingSystem.SQL{Path.DirectorySeparatorChar}Triggers{Path.DirectorySeparatorChar}trg_UpdateIsActiveOnExpiration.sql"
            };

            foreach (var path in sqlQueryPaths)
            {
                var sqlQuery = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, path));
                await connection.ExecuteAsync(sqlQuery);
            }

            _loggerService.LogSuccess("Database migration completed successfully!");
        }
    }
}
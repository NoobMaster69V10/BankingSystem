using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace BankingSystem.Infrastructure.Repository;

public class BankReportRepository : IBankReportRepository
{
    private readonly IDbConnection _connection;
    private readonly ILogger<BankReportRepository> _logger;

    public BankReportRepository(
        IDbConnection connection,
        ILogger<BankReportRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<int> GetUserCountAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "SELECT COUNT(*) FROM AspNetUsers";

            if (since.HasValue)
            {
                query += " WHERE RegistrationDate >= @Since";
            }

            var command = new CommandDefinition(query, new { Since = since }, cancellationToken: cancellationToken);

            return await _connection.QuerySingleAsync<int>(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count since {Since}", since);
            throw;
        }
    }

    public async Task<int> GetTransactionCountAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "SELECT COUNT(*) FROM AccountTransactions";

            if (since.HasValue)
            {
                query += " WHERE TransactionDate >= @Since";
            }

            var command = new CommandDefinition(query, parameters: new { Since = since }, cancellationToken:cancellationToken);

            return await _connection.QuerySingleAsync<int>(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction count since {Since}", since);
            throw;
        }
    }

    public async Task<Dictionary<Currency, decimal>> GetTransactionIncomeAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = @"
                SELECT ba.Currency, COALESCE(SUM(t.TransactionFee), 0) AS Income
                FROM AccountTransactions t
                JOIN BankAccounts ba ON t.FromAccountId = ba.BankAccountId";

            if (since.HasValue)
            {
                query += " Where t.TransactionDate >= @Since";
            }

            query += " GROUP BY ba.Currency";

            var command = new CommandDefinition(query, parameters: new { Since = since }, cancellationToken: cancellationToken);

            var results = await _connection.QueryAsync<(string Currency, decimal Income)>(command);

            return results
                .ToDictionary(
                    r => Enum.Parse<Currency>(r.Currency),
                    r => r.Income);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction income since {Since}", since);
            throw;
        }
    }

    public async Task<Dictionary<Currency, decimal>> GetAverageTransactionIncomeAsync(DateTime? since = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = @"
                SELECT ba.Currency, COALESCE(AVG(t.TransactionFee), 0) AS AvgIncome
                FROM AccountTransactions t
                JOIN BankAccounts ba ON t.FromAccountId = ba.BankAccountId";

            if (since.HasValue)
            {
                query += "Where t.TransactionDate >= @Since";
            }

            query += " GROUP BY ba.Currency";

            var command = new CommandDefinition(query, parameters: new { Since = since }, cancellationToken: cancellationToken);

            var results = await _connection.QueryAsync<(string Currency, decimal AvgIncome)>(command);

            return results
                .ToDictionary(
                    r => Enum.Parse<Currency>(r.Currency),
                    r => r.AvgIncome);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average transaction income since {Since}", since);
            throw;
        }
    }

    public async Task<IEnumerable<DailyTransactionReport>> GetDailyTransactionsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = @"
                SELECT  
                CAST(t.TransactionDate AS DATE) AS Date, 
                COUNT(*) AS Count,
                ba.Currency,
                COALESCE(SUM(t.Amount), 0) AS TotalAmount
                FROM AccountTransactions t
                JOIN BankAccounts ba ON t.FromAccountId = ba.BankAccountId
                WHERE t.TransactionDate >= DATEADD(DAY, @Days * -1, GETDATE())
                GROUP BY CAST(t.TransactionDate AS DATE), ba.Currency
                ORDER BY CAST(t.TransactionDate AS DATE)";

            var command = new CommandDefinition(query, parameters: new { Days = days }, cancellationToken: cancellationToken);

            var results = await _connection.QueryAsync<(DateTime Date, int Count, string Currency, decimal TotalAmount)>(command);

            return results
                .GroupBy(r => r.Date)
                .Select(g => new DailyTransactionReport
                {
                    Date = g.Key,
                    Count = g.Sum(r => r.Count),
                    TotalAmount = g
                        .ToDictionary(
                            r => Enum.Parse<Currency>(r.Currency),
                            r => r.TotalAmount)
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily transactions for last {Days} days", days);
            throw;
        }
    }

    public async Task<IEnumerable<AtmTransaction>> GetAllAtmTransactionsAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT at.Amount, ba.Currency 
            FROM AccountTransactions at
            JOIN BankAccounts ba ON at.FromAccountId = ba.BankAccountId
            Where TransactionType = 0";

        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        return await _connection.QueryAsync<AtmTransaction>(command);
    }
}
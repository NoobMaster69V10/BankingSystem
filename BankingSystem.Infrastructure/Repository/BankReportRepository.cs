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
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> GetUserCountAsync(DateTime? since = null)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM AspNetUsers";

            if (since.HasValue)
            {
                query += " WHERE RegistrationDate >= @Since";
                return await _connection.QuerySingleAsync<int>(query, new { Since = since.Value });
            }

            return await _connection.QuerySingleAsync<int>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count since {Since}", since);
            throw;
        }
    }

    public async Task<int> GetTransactionCountAsync(DateTime? since = null)
    {
        try
        {
            var query = "SELECT COUNT(*) FROM AccountTransactions";

            if (since.HasValue)
            {
                query += " WHERE TransactionDate >= @Since";
                return await _connection.QuerySingleAsync<int>(query, new { Since = since.Value });
            }

            return await _connection.QuerySingleAsync<int>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction count since {Since}", since);
            throw;
        }
    }

    public async Task<Dictionary<Currency, decimal>> GetTransactionIncomeAsync(DateTime? since = null)
    {
        try
        {
            string query = @"
                SELECT ba.Currency, COALESCE(SUM(t.TransactionFee), 0) AS Income
                FROM AccountTransactions t
                JOIN BankAccounts ba ON t.FromAccountId = ba.BankAccountId";

            if (since.HasValue)
            {
                query += " Where t.TransactionDate >= @Since";
            }

            query += " GROUP BY ba.Currency";

            var results = await _connection.QueryAsync<(string Currency, decimal Income)>(
                query,
                new { Since = since });

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

    public async Task<Dictionary<Currency, decimal>> GetAverageTransactionIncomeAsync(DateTime? since = null)
    {
        try
        {
            string query = @"
                SELECT ba.Currency, COALESCE(AVG(t.TransactionFee), 0) AS AvgIncome
                FROM AccountTransactions t
                JOIN BankAccounts ba ON t.FromAccountId = ba.BankAccountId";

            if (since.HasValue)
            {
                query += "Where t.TransactionDate >= @Since";
            }

            query += " GROUP BY ba.Currency";

            var results = await _connection.QueryAsync<(string Currency, decimal AvgIncome)>(
                query,
                new { Since = since });

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

    public async Task<IEnumerable<DailyTransactionReport>> GetDailyTransactionsAsync(int days = 30)
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

            var results =
                await _connection.QueryAsync<(DateTime Date, int Count, string Currency, decimal TotalAmount)>(
                    query,
                    new { Days = days });

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

    public async Task<IEnumerable<AtmTransaction>> GetAllAtmTransactionsAsync()
    {
        const string query = @"
        SELECT at.Amount, ba.Currency 
        FROM AccountTransactions at
        JOIN BankAccounts ba ON at.FromAccountId = ba.BankAccountId
        Where TransactionType = 0";

        return await _connection.QueryAsync<AtmTransaction>(query);
    }
}
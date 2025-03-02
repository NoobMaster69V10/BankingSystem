using Dapper;
using System.Data;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Infrastructure.Repository;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnection _connection;
    public ReportRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public async Task<int> GetNumberOfRegisteredUsersAsync(string dateFilter)
    {
        

        if (dateFilter == "current-year")
        {
            dateFilter = "YEAR(RegistrationDate) = YEAR(GETDATE())";
        }

        if (dateFilter == "last-year")
        {
            dateFilter = "RegistrationDate >= DATEADD(YEAR, -1, GETDATE())";
        }

        if (dateFilter == "last-month")
        {
            dateFilter = "RegistrationDate >= DATEADD(MONTH, -1, GETDATE())";
        }

        var query = $"SELECT COUNT(*) FROM AspNetUsers WHERE {dateFilter}";


        return await _connection.QuerySingleOrDefaultAsync<int>(query);

    }
    public async Task<int> GetNumberOfTransactionsAsync(string dateFilter)
    {
        if (dateFilter == "last-year")
        {
            dateFilter = "TransactionDate >= DATEADD(YEAR, -1, GETDATE())";
        }

        if (dateFilter == "half-year")
        {
            dateFilter = "TransactionDate >= DATEADD(MONTH, -6, GETDATE())";
        }

        if (dateFilter == "last-month")
        {
            dateFilter = "TransactionDate >= DATEADD(MONTH, -1, GETDATE())";
        }

        var query = $"SELECT COUNT(*) FROM AccountTransactions WHERE {dateFilter};";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<decimal?> GetTransactionsIncomeByCurrencyAsync(string dateFilter, string currency)
    {
        if (dateFilter == "last-year")
        {
            dateFilter = "TransactionDate >= DATEADD(YEAR, -1, GETDATE())";
        }

        if (dateFilter == "half-year")
        {
            dateFilter = "TransactionDate >= DATEADD(MONTH, -6, GETDATE())";
        }

        if (dateFilter == "last-month")
        {
            dateFilter = "TransactionDate >= DATEADD(MONTH, -1, GETDATE())";
        }

        var query = $"SELECT SUM(TransactionFee) FROM AccountTransactions WHERE Currency = @Currency AND {dateFilter};";

        return await _connection.QuerySingleOrDefaultAsync<decimal?>(query, new { Currency = currency });
    }

    public async Task<decimal?> GetAverageTransactionsIncomeByCurrencyAsync(string currency)
    {
        const string query = "SELECT AVG(TransactionFee) FROM AccountTransactions WHERE Currency = @Currency;";

        return await _connection.QuerySingleOrDefaultAsync<decimal?>(query, new { Currency = currency });
    }

    public async Task<IEnumerable<DailyTransaction>?> GetTransactionsChartForLastMonthAsync()
    {
        const string query = @"SELECT 
                                   TransactionDate,
                                   COUNT(*) AS TransactionsCount
                               FROM AccountTransactions
                               WHERE TransactionDate >= DATEADD(MONTH, -1, GETDATE())
                               GROUP BY TransactionDate
                               ORDER BY TransactionDate DESC;";

        return await _connection.QueryAsync<DailyTransaction>(query);
    }
}
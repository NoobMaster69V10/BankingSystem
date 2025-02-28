using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Microsoft.Data.SqlClient;

namespace BankingSystem.Infrastructure.Repository;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnection _connection;
    public ReportRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public async Task<int> GetNumberOfRegisteredUsersThisYearAsync()
    {
        const string query = @"SELECT COUNT(*)
                               FROM AspNetUsers 
                               WHERE YEAR(RegistrationDate) = YEAR(GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<int> GetNumberOfRegisteredUsersForLastYearAsync()
    {
        const string query = @"SELECT COUNT(*) 
                               FROM AspNetUsers
                               WHERE RegistrationDate >= DATEADD(YEAR, -1, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<int> GetNumberOfRegisteredUsersForLastMonthAsync()
    {
        const string query = @"SELECT COUNT(*) as NumberOfUserRegisteredForLastMonth
                               FROM AspNetUsers
                               WHERE RegistrationDate >= DATEADD(MONTH, -1, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<int> GetNumberOfTransactionsForLastYearAsync()
    {
        const string query = @"SELECT COUNT(*) as NumberOfTransactionsForLastYear
                               FROM AccountTransactions
                               WHERE TransactionDate >= DATEADD(YEAR, -1, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<int> GetNumberOfTransactionsForLastHalfYearAsync()
    {
        const string query = @"SELECT COUNT(*) as NumberOfTransactionsForLastHalfYear
                               FROM AccountTransactions
                               WHERE TransactionDate >= DATEADD(MONTH, -6, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<int> GetNumberOfTransactionsForLastMonthAsync()
    {
        const string query = @"SELECT COUNT(*) as NumberOfTransactionsForLastMonth
                               FROM AccountTransactions
                               WHERE TransactionDate >= DATEADD(MONTH, -1, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<int>(query);
    }

    public async Task<decimal?> GetTransactionsIncomeByCurrencyLastYearAsync(string currency)
    {
        const string query = @"SELECT SUM(TransactionFee) FROM AccountTransactions 
                               WHERE Currency = @Currency AND TransactionDate >= DATEADD(YEAR, -1, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<decimal?>(query, new { Currency = currency });
    }
    public async Task<decimal?> GetTransactionsIncomeByCurrencyLastHalfYearAsync(string currency)
    {
        const string query = @"SELECT SUM(TransactionFee) FROM AccountTransactions 
                               WHERE Currency = @Currency AND TransactionDate >= DATEADD(MONTH, -6, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<decimal?>(query, new { Currency = currency });
    }
    public async Task<decimal?> GetTransactionsIncomeByCurrencyLastMonthAsync(string currency)
    {
        const string query = @"SELECT SUM(TransactionFee) FROM AccountTransactions 
                               WHERE Currency = @Currency AND TransactionDate >= DATEADD(MONTH, -1, GETDATE());";

        return await _connection.QuerySingleOrDefaultAsync<decimal?>(query, new { Currency = currency });
    }

    public async Task<decimal?> GetAverageTransactionsIncomeByCurrencyAsync(string currency)
    {
        const string query = "SELECT AVG(TransactionFee) FROM AccountTransactions WHERE Currency = @Currency;";

        return await _connection.QuerySingleOrDefaultAsync<decimal?>(query, new { Currency = currency });
    }

    public async Task<IEnumerable<DailyTransactions>> GetTransactionsChartForLastMonthAsync()
    {
        const string query = @"SELECT 
                                   TransactionDate,
                                   COUNT(*) AS TransactionsCount
                               FROM AccountTransactions
                               WHERE TransactionDate >= DATEADD(MONTH, -1, GETDATE())
                               GROUP BY TransactionDate
                               ORDER BY TransactionDate DESC;";

        return await _connection.QueryAsync<DailyTransactions>(query);
    }
}
using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IReportRepository
{
    Task<int> GetNumberOfRegisteredUsersThisYearAsync();
    Task<int> GetNumberOfRegisteredUsersForLastYearAsync();
    Task<int> GetNumberOfRegisteredUsersForLastMonthAsync();
    Task<int> GetNumberOfTransactionsForLastYearAsync();
    Task<int> GetNumberOfTransactionsForLastHalfYearAsync();
    Task<int> GetNumberOfTransactionsForLastMonthAsync();
    Task<decimal?> GetTransactionsIncomeByCurrencyLastYearAsync(string currency);
    Task<decimal?> GetTransactionsIncomeByCurrencyLastHalfYearAsync(string currency);
    Task<decimal?> GetTransactionsIncomeByCurrencyLastMonthAsync(string currency);
    Task<decimal?> GetAverageTransactionsIncomeByCurrencyAsync(string currency);
    Task<IEnumerable<DailyTransactions>> GetTransactionsChartForLastMonthAsync();
}
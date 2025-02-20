using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IReportRepository
{
    Task<int> GetNumberOfRegisteredUsersThisYear();
    Task<int> GetNumberOfRegisteredUsersForLastYear();
    Task<int> GetNumberOfRegisteredUsersForLastMonth();
    Task<int> GetNumberOfTransactionsForLastYear();
    Task<int> GetNumberOfTransactionsForLastHalfYear();
    Task<int> GetNumberOfTransactionsForLastMonth();
    Task<decimal?> GetTransactionsIncomeByCurrencyLastYear(string currency);
    Task<decimal?> GetTransactionsIncomeByCurrencyLastHalfYear(string currency);
    Task<decimal?> GetTransactionsIncomeByCurrencyLastMonth(string currency);
    Task<decimal?> GetAverageTransactionsIncomeByCurrency(string currency);
    Task<IEnumerable<DailyTransactions>> GetTransactionsChartForLastMonth();
}
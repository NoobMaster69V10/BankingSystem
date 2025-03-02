using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IReportRepository
{
    Task<int> GetNumberOfRegisteredUsersAsync(string dateFilter);
    Task<int> GetNumberOfTransactionsAsync(string dateFilter);
    Task<decimal?> GetTransactionsIncomeByCurrencyAsync(string dateFilter, string currency);
    Task<decimal?> GetAverageTransactionsIncomeByCurrencyAsync(string currency);
    Task<IEnumerable<DailyTransaction>?> GetTransactionsChartForLastMonthAsync();
}
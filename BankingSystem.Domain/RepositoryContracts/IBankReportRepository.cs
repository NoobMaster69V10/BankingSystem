using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankReportRepository
{
    Task<int> GetUserCountAsync(DateTime? since = null);
    Task<int> GetTransactionCountAsync(DateTime? since = null);
    Task<Dictionary<Currency, decimal>> GetTransactionIncomeAsync(DateTime? since = null);
    Task<Dictionary<Currency, decimal>> GetAverageTransactionIncomeAsync(DateTime? since = null);
    Task<IEnumerable<DailyTransactionReport>> GetDailyTransactionsAsync(int days = 30);
}
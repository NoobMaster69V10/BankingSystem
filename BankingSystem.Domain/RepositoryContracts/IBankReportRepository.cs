using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankReportRepository
{
    Task<int> GetUserCountAsync(DateTime? since = null, CancellationToken cancellationToken = default);
    Task<int> GetTransactionCountAsync(DateTime? since = null, CancellationToken cancellationToken = default);
    Task<Dictionary<Currency, decimal>> GetTransactionIncomeAsync(DateTime? since = null, CancellationToken cancellationToken = default);
    Task<Dictionary<Currency, decimal>> GetAverageTransactionIncomeAsync(DateTime? since = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyTransactionReport>> GetDailyTransactionsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<AtmTransaction>> GetAllAtmTransactionsAsync(CancellationToken cancellationToken = default);
}
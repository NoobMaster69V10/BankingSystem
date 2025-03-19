using BankingSystem.Core.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Statistics;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankReportService
{
    Task<Result<BankManagerReport>> GetBankManagerReportAsync(CancellationToken cancellationToken = default);
    Task<Result<UserStatistics>> GetUserStatisticsAsync(CancellationToken cancellationToken = default);
    Task<Result<TransactionStatistics>> GetTransactionStatisticsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DailyTransactionReport>>> GetDailyTransactionsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<Result<AtmTransactionsStatistics>> GetAtmTransactionsStatisticsAsync(CancellationToken cancellationToken = default);
    
}
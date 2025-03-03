using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Statistics;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankReportService
{
    Task<Result<BankManagerReport>> GetBankManagerReportAsync();
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<TransactionStatistics>> GetTransactionStatisticsAsync();
    Task<Result<IEnumerable<DailyTransactionReport>>> GetDailyTransactionsAsync(int days = 30);
    Task<Result<AtmTransactionsStatistics>> GetAtmTransactionsStatisticsAsync();
    
}
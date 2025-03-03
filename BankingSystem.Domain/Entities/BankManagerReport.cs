using BankingSystem.Domain.Statistics;

namespace BankingSystem.Domain.Entities;

public class BankManagerReport
{
    public UserStatistics UserStats { get; set; } = new();
    public TransactionStatistics TransactionStats { get; set; } = new();
    public List<DailyTransactionReport> DailyTransactions { get; set; } = new();
    public AtmTransactionsStatistics AtmStats { get; set; } = new();
}
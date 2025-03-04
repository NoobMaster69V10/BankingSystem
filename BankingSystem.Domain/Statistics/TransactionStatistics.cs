using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Statistics;

public class TransactionStatistics
{
    public int TransactionsLastMonth { get; set; }
    public int TransactionsLastSixMonths { get; set; }
    public int TransactionsLastYear { get; set; }
    
    public Dictionary<Currency, decimal> IncomeLastMonth { get; set; } = new();
    public Dictionary<Currency, decimal> IncomeLastSixMonths { get; set; } = new();
    public Dictionary<Currency, decimal> IncomeLastYear { get; set; } = new();
    
    public Dictionary<Currency, decimal> AverageIncomePerTransaction { get; set; } = new();
}
namespace BankingSystem.Domain.Statistics;

public class AtmTransactionsStatistics
{
    public decimal TotalWithdrawnAmount { get; set; } = new();
    public string? Currency { get; set; }
}
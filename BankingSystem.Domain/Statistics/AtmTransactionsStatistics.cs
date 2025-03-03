using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Statistics;

public class AtmTransactionsStatistics
{
    public decimal TotalWithdrawnAmount { get; set; } = new();

}
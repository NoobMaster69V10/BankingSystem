namespace BankingSystem.Domain.Entities;

public class DailyTransaction
{
    public DateTime TransactionDate { get; set; }
    public int TransactionsCount { get; set; }
}
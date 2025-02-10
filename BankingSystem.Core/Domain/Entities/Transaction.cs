namespace BankingSystem.Core.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime TransactionDate { get; set; }
    public int FromAccountId { get; set; }
    public BankAccount FromAccount { get; set; }

    public int ToAccountId { get; set; }
    public BankAccount ToAccount { get; set; }
}
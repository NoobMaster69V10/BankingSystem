using BankingSystem.Core.Domain.Entities;

namespace BankingSystem.Core.DTO;

public class TransactionDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime TransactionDate { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
}
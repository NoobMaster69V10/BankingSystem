using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.Entities;

public class AtmTransaction
{
    [Required]
    public int Amount { get; set; }
    [Required]
    public string Currency { get; set; }
    public DateTime TransactionDate { get; set; }
    public int AccountId { get; set; }
}
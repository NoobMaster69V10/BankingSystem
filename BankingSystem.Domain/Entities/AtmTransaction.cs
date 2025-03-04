using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.Entities;

public class AtmTransaction
{
    [Key]
    public int AtmTransactionId { get; set; }
    [Required]
    public int Amount { get; set; }
    [Required]
    public string? Currency { get; set; }
    [Required]
    public DateTime TransactionDate { get; set; }
    [Required]
    public int AccountId { get; set; }
    [Required]
    public decimal TransactionFee { get; set; }
}
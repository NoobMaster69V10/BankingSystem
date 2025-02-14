using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.Entities;

public class AccountTransaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Transaction amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code.")]
    public required string Currency { get; set; }

    [Required(ErrorMessage = "Transaction date is required.")]
    public DateTime TransactionDate { get; set; }

    [Required(ErrorMessage = "From account ID is required.")]
    public int FromAccountId { get; set; }

    [Required(ErrorMessage = "To account ID is required.")]
    public int ToAccountId { get; set; }
}
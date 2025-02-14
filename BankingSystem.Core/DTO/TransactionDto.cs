using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public class TransactionDto
{
    [NegativeNumberValidation(ErrorMessage = "Amount cannot be negative.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "FromAccountId is required.")]
    public int FromAccountId { get; set; }

    [Required(ErrorMessage = "ToAccountId is required.")]
    public int ToAccountId { get; set; }
}
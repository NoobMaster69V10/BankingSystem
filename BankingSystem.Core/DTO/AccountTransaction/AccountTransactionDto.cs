using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO.AccountTransaction;

public record AccountTransactionDto
{
    [NegativeNumberValidation(ErrorMessage = "Amount cannot be negative.")]
    public decimal Amount { get; init; }

    [Required(ErrorMessage = "FromAccountId is required.")]
    public int FromAccountId { get; init; }

    [Required(ErrorMessage = "ToAccountId is required.")]
    public int ToAccountId { get; init; }
}
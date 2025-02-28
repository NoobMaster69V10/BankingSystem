using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public record BankAccountRegisterDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "IBAN is required.")]
    [IbanValidation(ErrorMessage = "Invalid IBAN format.")]
    public string Iban { get; init; } = string.Empty;

    [NegativeNumberValidation(ErrorMessage = "Balance cannot be negative.")]
    public decimal Balance { get; init; }

    [Required(ErrorMessage = "Currency is required.")]
    [AllowedValues("GEL", "USD", "EUR", ErrorMessage = "Currency must be GEL, USD, or EUR.")]
    public string Currency { get; init; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Core.DTO.BankAccount;

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
    [AllowedValues(Currency.USD, Currency.EUR, Currency.GEL)]
    public Currency Currency { get; init; }
}
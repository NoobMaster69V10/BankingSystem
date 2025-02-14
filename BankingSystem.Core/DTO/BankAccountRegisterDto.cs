using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public class BankAccountRegisterDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "IBAN is required.")]
    [IbanValidation(ErrorMessage = "Invalid IBAN format.")]
    public string Iban { get; set; }

    [NegativeNumberValidation(ErrorMessage = "Balance cannot be negative.")]
    public decimal Balance { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [AllowedValues("GEL", "USD", "EUR", ErrorMessage = "Currency must be GEL, USD, or EUR.")]
    public string Currency { get; set; }
}
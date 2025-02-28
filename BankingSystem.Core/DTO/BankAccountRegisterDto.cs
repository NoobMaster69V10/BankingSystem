using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public record BankAccountRegisterDto(
    [Required(ErrorMessage = "Username is required.")] string Username,
    
    [Required(ErrorMessage = "IBAN is required.")]
    [IbanValidation(ErrorMessage = "Invalid IBAN format.")] string Iban,
    
    [NegativeNumberValidation(ErrorMessage = "Balance cannot be negative.")] decimal Balance,
    
    [Required(ErrorMessage = "Currency is required.")]
    [AllowedValues("GEL", "USD", "EUR", ErrorMessage = "Currency must be GEL, USD, or EUR.")] string Currency
);
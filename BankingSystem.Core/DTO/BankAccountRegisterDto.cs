using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public class BankAccountRegisterDto
{
    public required string Username { get; set; }
    [IbanValidation]
    public required string Iban { get; set; }
    [NegativeNumberValidation]
    public required decimal Balance { get; set; }
    [AllowedValues("GEL", "USD", "EUR")]
    public required string Currency { get; set; }
}

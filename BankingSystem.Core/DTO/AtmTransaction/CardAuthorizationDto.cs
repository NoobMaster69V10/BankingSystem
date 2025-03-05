using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record CardAuthorizationDto
{
    [Required(ErrorMessage = "Card number is required.")]
    [CreditCard(ErrorMessage = "Invalid card number format.")]
    public string CardNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Current PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")]
    public string PinCode { get; init; } = string.Empty;
}
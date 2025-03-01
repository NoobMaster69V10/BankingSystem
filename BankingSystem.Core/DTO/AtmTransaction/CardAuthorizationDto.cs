using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record CardAuthorizationDto
{
    [Required(ErrorMessage = "Card number is required.")]
    public string CardNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Pin Codes is required.")]
    [StringLengthFixedValidation(4, ErrorMessage = "Pin code must be exactly 4 characters.")]
    public string PinCode { get; init; } = string.Empty;
}
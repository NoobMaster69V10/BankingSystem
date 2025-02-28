using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public record BankCardRegisterDto
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Card number is required.")]
    [Length(13, 19)]
    public string CardNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Firstname is required.")]
    public string Firstname { get; init; } = string.Empty;

    [Required(ErrorMessage = "Lastname is required.")]
    public string Lastname { get; init; } = string.Empty;

    [Required(ErrorMessage = "Expiration date is required.")]
    [CheckExpirationDateValidation]
    public DateTime ExpirationDate { get; init; }

    [StringLengthFixedValidation(3, ErrorMessage = "CVV must be exactly 3 characters.")]
    public string Cvv { get; init; } = string.Empty;

    [StringLengthFixedValidation(4, ErrorMessage = "Pin code must be exactly 4 characters.")]
    public string PinCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "BankAccountId is required.")]
    public int BankAccountId { get; init; }
}

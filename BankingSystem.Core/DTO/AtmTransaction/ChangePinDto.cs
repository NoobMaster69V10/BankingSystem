using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.AtmTransaction;


public record ChangePinDto
{
    public string CardNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Current PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")]
    public string CurrentPin { get; init; } = string.Empty;

    [Required(ErrorMessage = "New PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")]
    public string NewPin { get; init; } = string.Empty;
}
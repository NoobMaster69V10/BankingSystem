using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public record ChangePinDto(
    string CardNumber,
    
    [Required(ErrorMessage = "Current PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")] string CurrentPin,
    
    [Required(ErrorMessage = "New PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")] string NewPin
);
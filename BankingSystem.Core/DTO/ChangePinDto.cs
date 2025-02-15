using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class ChangePinDto
{
    [Required]
    public string CardNumber { get; set; }
    
    [Required(ErrorMessage = "New PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")]
    public string CurrentPin { get; set; }
    
    
    [Required(ErrorMessage = "PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN code must contain only digits.")]
    public string NewPin { get; set; }
}
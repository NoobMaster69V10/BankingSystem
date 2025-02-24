using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class ResetPasswordDto
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Token { get; set; }
    [Required]
    public string NewPassword { get; set; }
    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}
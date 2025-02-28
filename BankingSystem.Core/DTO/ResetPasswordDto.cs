using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public record ResetPasswordDto(
    [Required] string Email,

    [Required] string Token,

    [Required] string NewPassword)
{
    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
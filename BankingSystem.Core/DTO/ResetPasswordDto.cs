using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public record ResetPasswordDto
{
    [Required]
    public string Email { get; init; }

    [Required]
    public string Token { get; init; }

    [Required]
    public string NewPassword { get; init; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; }
}
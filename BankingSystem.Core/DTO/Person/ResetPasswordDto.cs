using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.Person;

public record ResetPasswordDto
{
    [Required]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
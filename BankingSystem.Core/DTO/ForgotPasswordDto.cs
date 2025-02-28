using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;


public record ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}
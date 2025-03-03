using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.Person;


public record ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    [Required]
    public string ClientUri { get; init; } = string.Empty;
}
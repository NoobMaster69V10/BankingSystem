using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
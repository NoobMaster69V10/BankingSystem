using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;


public record ForgotPasswordDto(
    [Required]
    [EmailAddress] string Email
);
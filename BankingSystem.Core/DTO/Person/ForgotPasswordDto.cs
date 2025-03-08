using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace BankingSystem.Core.DTO.Person;
public record ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    [Required]
    public string ClientUri { get; init; } = string.Empty;
}

internal sealed class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.ClientUri)
            .NotEmpty().WithMessage("Client URI is required.");
    }
}
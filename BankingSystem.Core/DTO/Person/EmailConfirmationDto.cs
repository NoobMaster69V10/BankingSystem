using FluentValidation;

namespace BankingSystem.Core.DTO.Person;

public record EmailConfirmationDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

internal sealed class EmailConfirmationDtoValidator : AbstractValidator<EmailConfirmationDto>
{
    public EmailConfirmationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
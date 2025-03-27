using FluentValidation;

namespace BankingSystem.Core.DTO.Person;
public record ForgotPasswordDto
{
    public string Email { get; init; } = string.Empty;
}

internal sealed class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
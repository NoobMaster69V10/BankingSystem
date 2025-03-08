using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace BankingSystem.Core.DTO.Person;

public record PersonLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
internal sealed class PersonLoginDtoValidator : AbstractValidator<PersonLoginDto>
{
    public PersonLoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Must((_, password) => !password.ToLower().Contains("password"))
            .WithMessage("The word 'password' is not allowed in the password.")
            .Must((dto, password) => !password.ToLower().Equals(dto.Email.ToLower()))
            .WithMessage("Password cannot be the same as the email.");
    }
}
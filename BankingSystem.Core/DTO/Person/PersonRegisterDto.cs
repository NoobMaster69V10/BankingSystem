using FluentValidation;

namespace BankingSystem.Core.DTO.Person;

public record PersonRegisterDto
{
    public string FirstName { get; init; } = string.Empty;
    public string Lastname { get; init; } = string.Empty;
    public string IdNumber { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ClientUri { get; set; }
}

internal sealed class PersonRegisterDtoValidator : AbstractValidator<PersonRegisterDto>
{
    public PersonRegisterDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required.");
        RuleFor(x => x.Lastname).NotEmpty().WithMessage("Last Name is required.");
        RuleFor(x => x.IdNumber)
            .NotEmpty().WithMessage("IdNumber is required.")
            .Length(11).WithMessage("IdNumber must be exactly 11 characters.");
        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth Date is required.")
            .LessThan(DateTime.Now).WithMessage("Birth Date cannot be in the future.")
            .Must(date => date != default(DateTime)).WithMessage("Invalid birth date format.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.Password).WithMessage("The password and confirmation password do not match.");
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role == "Operator" || role == "Person" || role == "Manager")
            .WithMessage("Role must be either 'Operator', 'Person', or 'Manager'.");
    }
}
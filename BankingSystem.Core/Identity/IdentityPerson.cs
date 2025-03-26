using FluentValidation;
using Microsoft.AspNetCore.Identity;
public class IdentityPerson : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public override string? Email { get; set; }
    
    public DateTime BirthDate { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}

namespace BankingSystem.Core.Identity
{
    public class IdentityPersonValidator : AbstractValidator<IdentityPerson>
    {
        public IdentityPersonValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50);

            RuleFor(x => x.Lastname)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50);

            RuleFor(x => x.IdNumber)
                .NotEmpty().WithMessage("Id Number is required")
                .Length(11).WithMessage("ID number must be exactly 11 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required");
        }
    }
}

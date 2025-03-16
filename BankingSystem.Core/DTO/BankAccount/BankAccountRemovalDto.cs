using FluentValidation;

namespace BankingSystem.Core.DTO.BankAccount;

public class BankAccountRemovalDto
{
    public string? Iban { get; init; }
    public string? PersonId { get; init; }
}

internal sealed class BankAccountRemovalDtoValidator : AbstractValidator<BankAccountRemovalDto>
{
    public BankAccountRemovalDtoValidator()
    {
        RuleFor(x => x.Iban)
            .NotEmpty().WithMessage("Iban is required.");
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("PersonId is required.");
    }
}
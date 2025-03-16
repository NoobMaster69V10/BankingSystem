using FluentValidation;

namespace BankingSystem.Core.DTO.BankCard;

public record BankCardActiveDto
{
    public string? CardNumber { get; init; }
    public string? PersonId { get; init; }
}

internal sealed class BankAccountRemovalDtoValidator : AbstractValidator<BankCardActiveDto>
{
    public BankAccountRemovalDtoValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("CardNumber is required.");
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("PersonId is required.");
    }
}
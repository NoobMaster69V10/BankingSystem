using FluentValidation;

namespace BankingSystem.Core.DTO.AtmTransaction;
public record ChangePinDto : CardAuthorizationDto
{ 
    public string NewPin { get; init; } = string.Empty;
}

internal sealed class ChangePinDtoValidator : AbstractValidator<ChangePinDto>
{
    public ChangePinDtoValidator()
    {
        RuleFor(x => x.PinCode)
            .NotEmpty().WithMessage("PIN code is required.")
            .Length(4).WithMessage("PIN code must be exactly 4 digits.");
        RuleFor(x => x)
            .Must(x => x.PinCode != x.NewPin)
            .WithMessage("FromAccount and ToAccount cannot be the same.");
    }
}
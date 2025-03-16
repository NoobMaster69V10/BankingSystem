using FluentValidation;

namespace BankingSystem.Core.DTO.BankCard;

public record BankCardRegisterDto
{
    public string? Username { get; init; }
    public string? CardNumber { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? Cvv { get; init; }
    public string PinCode { get; init; } = string.Empty;
    public int BankAccountId { get; init; }
}


internal sealed class BankCardRegisterDtoValidator : AbstractValidator<BankCardRegisterDto>
{
    public BankCardRegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .CreditCard().WithMessage("Invalid card number format.");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty().WithMessage("Expiration date is required.")
            .Must(date => date > DateTime.Now).WithMessage("Expiration date must be in the future.");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("Cvv is required.")
            .Length(4).WithMessage("Cvv must be exactly 3 digits.")
            .Matches(@"^\d{3}$").WithMessage("Cvv must contain only digits.");

        RuleFor(x => x.PinCode)
            .NotEmpty().WithMessage("PIN code is required.")
            .Length(4).WithMessage("PIN code must be exactly 4 digits.")
            .Matches(@"^\d{4}$").WithMessage("PIN code must contain only digits.");

        RuleFor(x => x.BankAccountId)
            .NotEmpty().WithMessage("BankAccountId is required.");
    }
}
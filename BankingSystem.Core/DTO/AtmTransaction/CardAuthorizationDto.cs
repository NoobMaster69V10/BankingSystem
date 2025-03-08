
using BankingSystem.Domain.RepositoryContracts;
using FluentValidation;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record CardAuthorizationDto
{
    public string CardNumber { get; init; } = string.Empty;
    public string PinCode { get; init; } = string.Empty;
}

internal sealed class CardAuthorizationDtoValidator : AbstractValidator<CardAuthorizationDto>
{
    public CardAuthorizationDtoValidator(IBankAccountRepository bankAccountRepository) 
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .CreditCard().WithMessage("Invalid card number format.");
        
        RuleFor(x => x.PinCode)
            .NotEmpty().WithMessage("PIN code is required.")
            .Length(4).WithMessage("PIN code must be exactly 4 digits.")
            .Matches(@"^\d{4}$").WithMessage("PIN code must contain only digits.");
    }
}
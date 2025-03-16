using FluentValidation;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record WithdrawMoneyDto : CardAuthorizationDto
{ 
    public int Amount { get; init; }
}

internal sealed class WithdrawMoneyDtoValidator : AbstractValidator<WithdrawMoneyDto>
{
    public WithdrawMoneyDtoValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .Must(amount => amount % 1 == 0).WithMessage("Withdrawals must be in whole numbers (paper money only).");
    }
}
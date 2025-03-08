using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record WithdrawMoneyDto : CardAuthorizationDto
{ 
    [Required(ErrorMessage = "Amount is required.")]
    public int Amount { get; init; }
}

internal sealed class WithdrawMoneyDtoValidator : AbstractValidator<WithdrawMoneyDto>
{
    public WithdrawMoneyDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .Must(amount => amount % 1 == 0).WithMessage("Withdrawals must be in whole numbers (paper money only).");
    }
}
using FluentValidation;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record DepositMoneyDto : CardAuthorizationDto
{ 
    public int Amount { get; init; }
}
internal sealed class DepositMoneyDtoValidator : AbstractValidator<DepositMoneyDto>
{
    public DepositMoneyDtoValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}
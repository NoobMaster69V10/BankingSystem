using FluentValidation;

namespace BankingSystem.Core.DTO.AccountTransaction;
public record AccountTransactionDto
{
    public decimal Amount { get; init; }
    public int FromAccountId { get; init; }
    public int ToAccountId { get; init; }
}

public sealed class AccountTransactionDtoValidator : AbstractValidator<AccountTransactionDto>
{
    public AccountTransactionDtoValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Amount cannot be empty")
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("From account cannot be empty")
            .GreaterThan(0).WithMessage("FromAccountId must be a positive number.");

        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("To account cannot be empty")
            .GreaterThan(0).WithMessage("ToAccountId must be a positive number.");

        RuleFor(x => x)
            .Must(x => x.FromAccountId != x.ToAccountId)
            .WithMessage("FromAccountId and ToAccountId cannot be the same.");
    }
}
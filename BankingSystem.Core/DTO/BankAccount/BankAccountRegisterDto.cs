using System.Numerics;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Core.DTO.BankAccount;

public record BankAccountRegisterDto
{
    public string Username { get; init; } = string.Empty;
    public string Iban { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public Currency Currency { get; init; }
}

internal sealed class BankAccountRegisterDtoValidator : AbstractValidator<BankAccountRegisterDto>
{
    private const string IbanPattern = @"^[A-Z]{2}\d{2}[A-Z0-9]{11,30}$";

    public BankAccountRegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Iban)
            .NotEmpty().WithMessage("IBAN is required.")
            .Matches(IbanPattern).WithMessage("Invalid IBAN format.")
            .Must(ValidateIbanChecksum).WithMessage("Invalid IBAN checksum.");

        RuleFor(x => x.Balance)
            .NotEmpty().WithMessage("Balance is required.")
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .IsInEnum().WithMessage("Invalid currency value.");
    }

    private bool ValidateIbanChecksum(string iban)
    {
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);
        var numericIban = string.Concat(rearranged.Select(c => char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString()));

        return BigInteger.Parse(numericIban) % 97 == 1;
    }
}

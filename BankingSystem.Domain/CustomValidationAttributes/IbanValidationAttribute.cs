using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.RegularExpressions;

namespace BankingSystem.Domain.CustomValidationAttributes;
public class IbanValidationAttribute : ValidationAttribute
{
    private const string IbanPattern = @"^[A-Z]{2}\d{2}[A-Z0-9]{11,30}$";

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null!)
            return new ValidationResult("IBAN is required.");

        var iban = value.ToString()!.Replace(" ", "").ToUpper();

        if (!Regex.IsMatch(iban, IbanPattern))
            return new ValidationResult("Invalid IBAN format.");

        return !ValidateIbanChecksum(iban) ? new ValidationResult("Invalid IBAN checksum.") : ValidationResult.Success!;
    }

    private static bool ValidateIbanChecksum(string iban)
    {
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);

        var numericIban = string.Concat(rearranged.Select(c => char.IsLetter(c) ? (c - 'A' + 10).ToString() : c.ToString()));

        return BigInteger.Parse(numericIban) % 97 == 1;
    }
}
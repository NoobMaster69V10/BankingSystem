using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.CustomValidationAttributes;

public class StringLengthFixedValidationAttribute(int fixedLength) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is string stringValue)
        {
            if (stringValue.Length == fixedLength)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"The field {validationContext.DisplayName} must be length of '{fixedLength}'.");

        }

        return new ValidationResult("Invalid data type, must be integer.");
    }
}
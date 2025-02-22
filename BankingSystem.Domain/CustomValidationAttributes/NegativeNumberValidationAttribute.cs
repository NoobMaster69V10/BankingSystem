using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.CustomValidationAttributes;

public class NegativeNumberValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null!)
            return new ValidationResult("Number is required.");

        return value is decimal and < 0 ? new ValidationResult("Number must be 0 or positive.") : ValidationResult.Success!;
    }
}
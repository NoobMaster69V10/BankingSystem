using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.CustomValidationAttributes;

public class NegativeNumberValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null!)
            return new ValidationResult("Number is required.");


        if (value is decimal and < 0)
            return new ValidationResult("Number must be 0 or positive.");

        return ValidationResult.Success!;
    }
}
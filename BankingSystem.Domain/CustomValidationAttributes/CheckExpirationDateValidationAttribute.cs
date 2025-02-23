using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.CustomValidationAttributes;

public class CheckExpirationDateValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if(value is DateTime expirationDate)
        {
            if (expirationDate < DateTime.Now)
            {
                return new ValidationResult("Expiration date must be in the future.");
            }
        }
        else
        {
            return new ValidationResult("Invalid expiration date format.");
        }

        return ValidationResult.Success!;
    }

}
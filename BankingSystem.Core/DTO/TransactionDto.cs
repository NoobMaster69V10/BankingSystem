using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public record TransactionDto(
    [NegativeNumberValidation(ErrorMessage = "Amount cannot be negative.")] decimal Amount,
    
    [Required(ErrorMessage = "FromAccountId is required.")] int FromAccountId,
    
    [Required(ErrorMessage = "ToAccountId is required.")] int ToAccountId
);
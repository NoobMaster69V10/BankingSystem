using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public class TransactionDto
{
    [NegativeNumberValidation]
    public decimal Amount { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
}
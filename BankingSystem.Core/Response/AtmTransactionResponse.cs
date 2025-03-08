using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Core.DTO.Response;

public class AtmTransactionResponse
{
    [Required] public int Amount { get; init; }
    [Required] public Currency Currency { get; init; }
    [Required] public string Iban { get; init; } = string.Empty;
}
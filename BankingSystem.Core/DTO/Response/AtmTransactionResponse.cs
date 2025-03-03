using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.Response;

public class AtmTransactionResponse
{
    [Required] public int Amount { get; init; }
    [Required] public string Currency { get; init; } = string.Empty;
    [Required] public string Iban { get; init; } = string.Empty;
}
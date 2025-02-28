using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public record WithdrawMoneyDto
{
    [Required] public string CardNumber { get; init; } = string.Empty;
    [Required] public string PinCode { get; init; } = string.Empty;
    [Required] public int Amount { get; init; }
    [Required] public string Currency { get; init; } = string.Empty;
}
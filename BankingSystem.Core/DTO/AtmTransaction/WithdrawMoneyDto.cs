using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record WithdrawMoneyDto
{
    [Required] public string CardNumber { get; init; } = string.Empty;
    [Required] public string PinCode { get; init; } = string.Empty;
    [Required] public int Amount { get; init; }
}
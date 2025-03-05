using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record WithdrawMoneyDto : CardAuthorizationDto
{
    [Required] public int Amount { get; init; }
}
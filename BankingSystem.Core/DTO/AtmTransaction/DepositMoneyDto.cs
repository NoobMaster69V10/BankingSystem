using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.AtmTransaction;

public record DepositMoneyDto : CardAuthorizationDto
{
    [Required] public int Amount { get; init; }
}
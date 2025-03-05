using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO.AtmTransaction;


public record ChangePinDto : CardAuthorizationDto
{ 
    public string NewPin { get; init; } = string.Empty;
}
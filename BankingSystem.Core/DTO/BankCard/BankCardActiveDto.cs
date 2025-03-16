namespace BankingSystem.Core.DTO.BankCard;

public record BankCardActiveDto()
{
    public string CardNumber { get; init; }
    public string PersonId { get; init; }
};
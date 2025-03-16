namespace BankingSystem.Core.DTO;

public record BankCardActiveDto()
{
    public string CardNumber { get; init; }
    public string PersonId { get; init; }
};
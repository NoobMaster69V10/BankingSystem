namespace BankingSystem.Core.Response;

public record BalanceResponse
{
    public decimal Balance { get; set; }
    public string? CardNumber { get; set; }
}
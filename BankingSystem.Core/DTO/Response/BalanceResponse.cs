namespace BankingSystem.Core.DTO.Response;

public record BalanceResponse
{
    public decimal Balance { get; set; }
    public string CardNumber { get; set; }
    
}
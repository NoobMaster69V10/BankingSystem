namespace BankingSystem.Core.DTO.Response;

public class BalanceResponseDto
{
    public decimal Balance { get; set; }
    public string CardNumber { get; set; }

    public BalanceResponseDto(decimal balance, string cardNumber)
    {
        Balance = balance;
        CardNumber = cardNumber;
    }
}
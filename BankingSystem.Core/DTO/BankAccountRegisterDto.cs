namespace BankingSystem.Core.DTO;

public class BankAccountRegisterDto
{
    public string IBAN { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public int CustomerId { get; set; }
}

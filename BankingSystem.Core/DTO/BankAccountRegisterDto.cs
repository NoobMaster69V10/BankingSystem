namespace BankingSystem.Core.DTO;

public class BankAccountRegisterDto
{
    public string IBAN { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }
}

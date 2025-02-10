namespace BankingSystem.Core.DTO;

public class BankCardRegisterDto
{
    public string CardNumber { get; set; }
    public string Name { get; set; }
    public string Lastname { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string CVV { get; set; }
    public string PinCode { get; set; }
}


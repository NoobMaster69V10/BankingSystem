namespace BankingSystem.Core.Response;

public record AccountRemovalResponse : ResponseMessage
{
    public string Iban { get; set; } = string.Empty;
}
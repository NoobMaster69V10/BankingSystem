
namespace BankingSystem.Core.Response;

public record CardRemovalResponse : ResponseMessage
{ 
    public string CardNumber { get; set; } = string.Empty;
}
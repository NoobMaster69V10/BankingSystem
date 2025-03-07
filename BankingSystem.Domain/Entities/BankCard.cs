using System.Text.Json.Serialization;

namespace BankingSystem.Domain.Entities;

public class BankCard
{
    [JsonIgnore]
    public int BankCardId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public string Cvv { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public int AccountId { get; set; }
}
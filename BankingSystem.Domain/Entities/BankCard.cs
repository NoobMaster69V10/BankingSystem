using System.Text.Json.Serialization;
using BankingSystem.Domain.Helpers;

namespace BankingSystem.Domain.Entities;

public class BankCard
{
    [JsonIgnore]
    public int BankCardId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    [JsonIgnore]
    public string Cvv { get; set; } = string.Empty;
    [JsonIgnore]
    public string PinCode { get; set; } = string.Empty;
    public int AccountId { get; set; }
    [JsonPropertyName("IsActive")]
    public bool BankCardStatus { get; set; } = true;
    [JsonPropertyName("cvv")]
    public string ConvertedCvv => EncryptionHelper.Decrypt(Cvv);
}
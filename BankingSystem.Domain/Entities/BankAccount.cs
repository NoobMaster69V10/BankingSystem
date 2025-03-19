using System.Text.Json.Serialization;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class BankAccount
{
    public int BankAccountId { get; set; }
    public string? Iban { get; set; }
    public decimal Balance { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Currency Currency { get; set; }

    [JsonIgnore]
    public string PersonId { get; set; } = string.Empty;
    [JsonPropertyName("IsActive")]
    public bool BankAccountStatus { get; set; } = true;
}
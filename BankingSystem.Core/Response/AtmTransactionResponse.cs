using System.Text.Json.Serialization;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Core.Response;

public class AtmTransactionResponse
{
    public int Amount { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Currency Currency { get; init; }
    public string Iban { get; init; } = string.Empty;
}
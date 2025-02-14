using System.Text.Json.Serialization;

namespace BankingSystem.Infrastructure.Data.ExternalApis;
public class CurrencyResponse
{
    [JsonPropertyName("currencies")]
    public List<Currency>? Currencies { get; set; }
}
public class Currency
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }
}

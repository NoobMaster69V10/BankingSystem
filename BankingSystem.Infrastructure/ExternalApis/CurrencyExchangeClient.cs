using System.Text.Json;
using BankingSystem.Domain.ExternalApiContracts;

namespace BankingSystem.Infrastructure.ExternalApis;
public class CurrencyExchangeClient : ICurrencyExchangeClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CurrencyExchangeClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<decimal> GetExchangeRateAsync(Domain.Enums.Currency currency, CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://nbg.gov.ge/gw/api/ct/monetarypolicy/currencies/ka/json/?currencies={currency.ToString()}");

        var client = _httpClientFactory.CreateClient();
        var httpResponseMessage = await client.SendAsync(httpRequestMessage, cancellationToken);

        IEnumerable<CurrencyResponse>? currencyResponses = new List<CurrencyResponse>();

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            await using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

            currencyResponses = await JsonSerializer.DeserializeAsync<IEnumerable<CurrencyResponse>>(contentStream, cancellationToken: cancellationToken);
        }

        return currencyResponses!.First().Currencies!.First().Rate;
    }
}
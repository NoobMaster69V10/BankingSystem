using System.Text.Json;
using BankingSystem.Domain.ConfigurationSettings.CurrencyExchangeClient;
using BankingSystem.Domain.ExternalApiContracts;
using Microsoft.Extensions.Options;

namespace BankingSystem.Infrastructure.ExternalApis;
public class CurrencyExchangeClient : ICurrencyExchangeClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CurrencyExchangeClientSettings _clientOptions;
    public CurrencyExchangeClient(IHttpClientFactory httpClientFactory, IOptions<CurrencyExchangeClientSettings> clientOptions)
    {
        _httpClientFactory = httpClientFactory;
        _clientOptions = clientOptions.Value;
    }

    public async Task<decimal> GetExchangeRateAsync(Domain.Enums.Currency currency, CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_clientOptions.ApiUrl}?currencies={currency.ToString()}");

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
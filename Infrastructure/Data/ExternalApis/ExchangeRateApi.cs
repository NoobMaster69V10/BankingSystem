using System.Text.Json;
using BankingSystem.Domain.ExternalApiContracts;

namespace BankingSystem.Infrastructure.Data.ExternalApis;

public class ExchangeRateApi(IHttpClientFactory httpClientFactory) : IExchangeRateApi
{
    public async Task<decimal> GetExchangeRate(string currency)
    {
        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://nbg.gov.ge/gw/api/ct/monetarypolicy/currencies/ka/json/?currencies={currency}");

        var client = httpClientFactory.CreateClient();
        var httpResponseMessage = await client.SendAsync(httpRequestMessage);

        IEnumerable<CurrencyResponse>? currencyResponses = new List<CurrencyResponse>();

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            currencyResponses = await JsonSerializer.DeserializeAsync<IEnumerable<CurrencyResponse>>(contentStream);
        }

        return currencyResponses!.First().Currencies!.First().Rate;
    }
}
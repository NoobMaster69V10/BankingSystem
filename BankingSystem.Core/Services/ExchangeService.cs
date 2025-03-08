using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.ExternalApiContracts;

namespace BankingSystem.Core.Services;

public class ExchangeService(IExchangeRateApi exchangeRateApi) : IExchangeService
{
    public async Task<decimal> ConvertCurrencyAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rates = new Dictionary<Currency, decimal>
        {
            { Currency.USD, await exchangeRateApi.GetExchangeRate(Currency.USD) },
            { Currency.EUR, await exchangeRateApi.GetExchangeRate(Currency.EUR) }
        };

        return fromCurrency switch
        {
            Currency.GEL when rates.ContainsKey(toCurrency) => amount / rates[toCurrency],
            Currency.USD when toCurrency == Currency.GEL => amount * rates[Currency.USD],
            Currency.EUR when toCurrency == Currency.GEL => amount * rates[Currency.EUR],
            Currency.USD when toCurrency == Currency.EUR => amount * (rates[Currency.EUR] / rates[Currency.USD]),
            Currency.EUR when toCurrency == Currency.USD => amount * (rates[Currency.USD] / rates[Currency.EUR]),
            _ => amount
        };
    }
}
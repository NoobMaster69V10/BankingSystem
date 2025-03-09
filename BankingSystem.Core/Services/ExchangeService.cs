using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.ExternalApiContracts;

namespace BankingSystem.Core.Services;

public class ExchangeService : IExchangeService
{
    private readonly ICurrencyExchangeClient _currencyExchangeClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeService> _logger;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
    
    public ExchangeService(
        ICurrencyExchangeClient currencyExchangeClient,
        IMemoryCache cache,
        ILogger<ExchangeService> logger)
    {
        _currencyExchangeClient = currencyExchangeClient;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<decimal> ConvertCurrencyAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rates = await GetCachedRatesAsync();

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
    
    private async Task<Dictionary<Currency, decimal>> GetCachedRatesAsync()
    {
        const string cacheKey = "ExchangeRates";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<Currency, decimal> cachedRates))
        {
            return cachedRates;
        }
        
        await _cacheLock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(cacheKey, out Dictionary<Currency, decimal> ratesAfterLock))
            {
                return ratesAfterLock;
            }
            
            var usdTask = _currencyExchangeClient.GetExchangeRate(Currency.USD);
            var eurTask = _currencyExchangeClient.GetExchangeRate(Currency.EUR);
            
            await Task.WhenAll(usdTask, eurTask);
            
            var rates = new Dictionary<Currency, decimal>
            {
                { Currency.USD, await usdTask },
                { Currency.EUR, await eurTask }
            };
            
            _cache.Set(cacheKey, rates, _cacheExpiration);
            
            _logger.LogInformation("Exchange rates refreshed and cached for {Duration}", _cacheExpiration);
            
            return rates;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
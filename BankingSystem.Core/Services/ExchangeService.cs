using Microsoft.Extensions.Caching.Memory;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.ExternalApiContracts;

namespace BankingSystem.Core.Services;

public class ExchangeService : IExchangeService
{
    private readonly ICurrencyExchangeClient _currencyExchangeClient;
    private readonly IMemoryCache _cache;
    private readonly ILoggerService _logger;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromDays(1);
    
    public ExchangeService(ICurrencyExchangeClient currencyExchangeClient, IMemoryCache cache, ILoggerService logger)
    {
        _currencyExchangeClient = currencyExchangeClient;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<decimal> ConvertCurrencyAsync(decimal amount, Currency fromCurrency, Currency toCurrency, CancellationToken cancellationToken = default)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rates = await GetCachedRatesAsync(cancellationToken);

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
    
    private async Task<Dictionary<Currency, decimal>> GetCachedRatesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "ExchangeRates";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<Currency, decimal>? cachedRates))
        {
            return cachedRates!;
        }
        
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out Dictionary<Currency, decimal>? ratesAfterLock))
            {
                return ratesAfterLock!;
            }
            
            var usdTask = _currencyExchangeClient.GetExchangeRateAsync(Currency.USD, cancellationToken);
            var eurTask = _currencyExchangeClient.GetExchangeRateAsync(Currency.EUR, cancellationToken);
            
            await Task.WhenAll(usdTask, eurTask);
            
            var rates = new Dictionary<Currency, decimal>
            {
                { Currency.USD, usdTask.Result },
                { Currency.EUR, eurTask.Result }
            };
            
            _cache.Set(cacheKey, rates, _cacheExpiration);
            
            _logger.LogSuccess("Exchange rates refreshed and cached for {Duration} " + _cacheExpiration);
            
            return rates;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
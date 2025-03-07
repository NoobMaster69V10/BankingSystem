using BankingSystem.Domain.Enums;

namespace BankingSystem.Core.ServiceContracts;

public interface IExchangeService
{ 
    Task<decimal> ConvertCurrencyAsync(decimal amount, Currency fromCurrency, Currency toCurrency);
}
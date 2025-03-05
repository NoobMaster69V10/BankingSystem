using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.ExternalApiContracts;

public interface IExchangeRateApi
{
    Task<decimal> GetExchangeRate(Currency currency);
}

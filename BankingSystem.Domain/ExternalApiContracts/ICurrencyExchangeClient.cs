using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.ExternalApiContracts;

public interface ICurrencyExchangeClient
{
    Task<decimal> GetExchangeRate(Currency currency);
}

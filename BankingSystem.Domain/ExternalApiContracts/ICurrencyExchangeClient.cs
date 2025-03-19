using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.ExternalApiContracts;

public interface ICurrencyExchangeClient
{
    Task<decimal> GetExchangeRateAsync(Currency currency, CancellationToken cancellationToken = default);
}

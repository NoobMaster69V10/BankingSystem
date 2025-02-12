namespace BankingSystem.Domain.ExternalApiContracts;

public interface IExchangeRateApi
{
    Task<decimal> GetExchangeRate(string currency);
}
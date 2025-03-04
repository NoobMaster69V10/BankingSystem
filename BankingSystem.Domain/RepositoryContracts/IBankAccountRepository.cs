using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankAccountRepository : IGenericRepository<BankAccount>
{
    Task UpdateBalanceAsync(BankAccount? account, decimal balance);
    Task<BankAccount?> GetAccountByIbanAsync(string iban);
    Task<string> GetAccountCurrencyAsync(int accountId);
}

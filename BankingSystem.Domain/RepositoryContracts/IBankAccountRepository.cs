using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankAccountRepository : IRepositoryBase
{
    Task AddBankAccountAsync(BankAccount account);
    Task<BankAccount?> GetAccountByIdAsync(int accountId);
    Task UpdateBalanceAsync(BankAccount? account);
    Task<BankAccount?> GetAccountByIbanAsync(string iban);
    Task<string> GetAccountCurrencyAsync(int accountId);
}

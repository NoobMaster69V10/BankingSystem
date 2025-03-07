using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankAccountRepository : IRepositoryBase
{
    Task AddBankAccountAsync(BankAccount account);
    Task<BankAccount?> GetAccountByIdAsync(int accountId);
    Task UpdateBalanceAsync(BankAccount? account);
    Task<BankAccount?> GetAccountByIbanAsync(string iban);
    Task<Currency> GetAccountCurrencyAsync(int accountId);
}

using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankAccountRepository : ITransaction
{
    Task CreateAccountAsync(BankAccount account);

    Task UpdateAccountAsync(BankAccount account);

    Task<BankAccount> GetAccountByIdAsync(int id);
    Task<IEnumerable<BankAccount>> GetAccountsByIdAsync(int id);
    Task UpdateBalanceAsync(BankAccount? account, decimal balance);
}

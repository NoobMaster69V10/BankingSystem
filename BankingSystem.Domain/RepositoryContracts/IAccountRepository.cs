using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountRepository : ITransaction
{
    Task CreateAccountAsync(BankAccount account);

    Task UpdateAccountAsync(BankAccount account);

    Task<BankAccount> GetAccountByIdAsync(int id);
    Task<IEnumerable<BankAccount>> GetAccountsByIdAsync(int id);
}

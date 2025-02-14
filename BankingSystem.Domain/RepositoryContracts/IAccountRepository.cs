using BankingSystem.Domain.Entities;
using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountRepository : ITransaction
{
    Task CreateAccountAsync(BankAccount account);

    Task UpdateAccountAsync(BankAccount account);

    Task<BankAccount> GetAccountByIdAsync(int id);
}

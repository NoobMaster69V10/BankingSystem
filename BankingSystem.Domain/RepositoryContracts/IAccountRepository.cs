using BankingSystem.Domain.Entities;
using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountRepository
{
    Task CreateAccountAsync(BankAccount account);

    Task UpdateAccountAsync(BankAccount account);

    Task<BankAccount> GetAccountByIdAsync(int id);
    void SetTransaction(IDbTransaction transaction);
}
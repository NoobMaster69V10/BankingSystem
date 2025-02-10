using BankingSystem.Core.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace BankingSystem.Core.Domain.RepositoryContracts;

public interface IAccountRepository
{
    Task CreateAccountAsync(BankAccount account);

    Task UpdateAccountAsync(BankAccount account, SqlTransaction transaction);

    Task<BankAccount> GetAccountByIdAsync(int id, SqlTransaction transaction);
}
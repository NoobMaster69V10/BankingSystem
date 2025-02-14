using BankingSystem.Domain.Entities;
using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountTransactionRepository : ITransaction
{
    Task AddAccountTransactionAsync(AccountTransaction accountTransaction);
}
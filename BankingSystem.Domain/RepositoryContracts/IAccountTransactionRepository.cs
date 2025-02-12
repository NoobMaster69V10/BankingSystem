using BankingSystem.Domain.Entities;
using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountTransactionRepository
{
    Task AddAccountTransactionAsync(AccountTransaction accountTransaction);
    void SetTransaction(IDbTransaction transaction);
}
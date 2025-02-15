using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountTransactionRepository : ITransaction
{
    Task AddAccountTransactionAsync(AccountTransaction accountTransaction);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction);
}
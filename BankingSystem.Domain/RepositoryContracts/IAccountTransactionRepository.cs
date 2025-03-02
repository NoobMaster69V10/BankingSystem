using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountTransactionRepository : IGenericRepository<AccountTransaction>
{
    Task AddAccountTransactionAsync(AccountTransaction transactionObj);
}
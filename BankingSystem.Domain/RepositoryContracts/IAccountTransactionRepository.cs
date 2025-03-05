using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountTransactionRepository : ITransactionalRepositoryBase
{
    Task AddAccountTransactionAsync(AccountTransaction transactionObj);
    Task<decimal> GetTotalWithdrawnTodayAsync(int accountId);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction);
}
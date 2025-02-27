using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IAccountTransactionRepository 
{
    Task AddAccountTransactionAsync(AccountTransaction accountTransaction);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction);
    Task<int> GetTotalWithdrawnTodayAsync(int accountId);
}
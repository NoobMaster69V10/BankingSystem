using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankTransactionRepository : IRepositoryBase
{
    Task AddAccountTransferAsync(AccountTransfer transferObj);
    Task<decimal> GetTotalWithdrawnTodayAsync(int accountId);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction);
}
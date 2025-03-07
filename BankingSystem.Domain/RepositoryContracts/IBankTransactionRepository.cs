using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankTransactionRepository : IRepositoryBase
{
    Task AddAccountTransferAsync(AccountTransfer transferObj, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalWithdrawnTodayAsync(int accountId, CancellationToken cancellationToken = default);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction, CancellationToken cancellationToken = default);
}
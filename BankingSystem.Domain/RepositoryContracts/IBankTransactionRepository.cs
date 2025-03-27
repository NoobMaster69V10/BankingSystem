using BankingSystem.Domain.Entities;
namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankTransactionRepository : IRepositoryBase
{
    Task<decimal> GetTotalWithdrawnTodayAsync(int accountId, CancellationToken cancellationToken = default);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction, CancellationToken cancellationToken = default);
    Task TransferBetweenAccountsAsync(AccountTransfer transferObj, decimal convertedAmount, CancellationToken cancellationToken = default);
}
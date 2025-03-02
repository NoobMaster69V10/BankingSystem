using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAtmRepository : IGenericRepository<AtmTransaction>
{
    Task<decimal> GetTotalWithdrawnTodayAsync(int accountId);
    Task AddAtmTransactionAsync(AtmTransaction atmTransaction);
    Task<IEnumerable<AtmTransaction>> GetAllAtmTransactionsAsync();
}
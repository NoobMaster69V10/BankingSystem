using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IAtmRepository : IGenericRepository<AtmTransaction>
{
    Task<int> GetTotalWithdrawnTodayAsync(int accountId);
}
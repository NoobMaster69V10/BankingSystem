using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IPersonRepository : ITransactionalRepositoryBase
{
    Task<Person?> GetByUsernameAsync(string username);
    Task<Person?> GetByIdAsync(string id);
}
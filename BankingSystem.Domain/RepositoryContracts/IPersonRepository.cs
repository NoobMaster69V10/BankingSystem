using BankingSystem.Domain.Entities;
using System.Threading;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IPersonRepository : IRepositoryBase
{
    Task<Person?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Person?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
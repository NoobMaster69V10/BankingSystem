using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IPersonRepository : ITransaction
{
    Task<Person?> GetPersonByUsernameAsync(string username);
    Task<Person?> GetPersonByIdAsync(string id);
}
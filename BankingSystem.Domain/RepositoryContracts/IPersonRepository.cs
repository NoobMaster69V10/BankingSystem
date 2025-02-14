using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IPersonRepository : ITransaction
{
    Task<Person?> GetUserByUsernameAsync(string username);
    Task<Person?> GetPersonByIdAsync(string id);
}
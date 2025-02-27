using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IPersonRepository 
{
    Task<Person?> GetPersonByUsernameAsync(string username);
    Task<Person?> GetPersonByIdAsync(string id);
}
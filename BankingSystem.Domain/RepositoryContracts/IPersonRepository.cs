using System.Data;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IUserRepository : ITransaction
{
    Task<Person?> GetUserByUsernameAsync(string username);
}
using System.Data;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IPersonRepository
{
    Task<Person?> GetUserByUsernameAsync(string username);
    void SetTransaction(IDbTransaction transaction);
    Task<Person?> GetUserByIdAsync(string id);
}
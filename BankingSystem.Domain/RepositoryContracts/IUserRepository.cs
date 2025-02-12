using System.Data;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IUserRepository
{
    Task<Person?> GetUserByUsernameAsync(string username);
    void SetTransaction(IDbTransaction transaction);
}
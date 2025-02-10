using BankingSystem.Core.Identity;

namespace BankingSystem.Core.Domain.RepositoryContracts;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
}
using BankingSystem.Core.Domain.Entities;

namespace BankingSystem.Core.Domain.RepositoryContracts;

public interface IAccountRepository
{
    Task CreateAccountAsync(BankAccount account);
}
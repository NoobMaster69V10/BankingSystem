using BankingSystem.Core.Domain.Entities;

namespace BankingSystem.Core.Domain.RepositoryContracts;

public interface ICardRepository
{
    Task CreateCardAsync(Card card);
}
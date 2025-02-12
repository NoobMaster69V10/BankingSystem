using BankingSystem.Domain.Entities;
using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IBankCardRepository
{
    Task CreateCardAsync(BankCard card);
    void SetTransaction(IDbTransaction transaction);
}
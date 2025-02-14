using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface ITransaction
{
    void SetTransaction(IDbTransaction transaction);
}
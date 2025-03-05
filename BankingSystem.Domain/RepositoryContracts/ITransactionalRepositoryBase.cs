using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface ITransactionalRepositoryBase
{
    void SetTransaction(IDbTransaction? transaction);
}
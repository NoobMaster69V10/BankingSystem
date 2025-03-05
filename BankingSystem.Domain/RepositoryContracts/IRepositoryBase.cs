using System.Data;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IRepositoryBase
{
    void SetTransaction(IDbTransaction? transaction);
}
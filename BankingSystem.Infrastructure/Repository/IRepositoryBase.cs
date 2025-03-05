using Dapper;
using System.Data;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public abstract class RepositoryBase : IRepositoryBase
{
    protected readonly IDbConnection Connection;
    protected IDbTransaction? Transaction;

    protected RepositoryBase(IDbConnection connection)
    {
        Connection = connection;
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        Transaction = transaction;
    }
}

using Dapper;
using System.Data;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public abstract class TransactionalRepositoryBase : ITransactionalRepositoryBase
{
    protected readonly IDbConnection Connection;
    protected IDbTransaction? Transaction;

    protected TransactionalRepositoryBase(IDbConnection connection)
    {
        Connection = connection;
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        Transaction = transaction;
    }
}

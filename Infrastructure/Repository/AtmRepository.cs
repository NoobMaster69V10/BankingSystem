using System.Data;

namespace BankingSystem.Infrastructure.Repository;

public class AtmRepository
{
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;

    public AtmRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }
    
}
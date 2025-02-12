using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Data.Repository;

public class BankCardRepository : IBankCardRepository
{
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;

    public BankCardRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }
    public async Task CreateCardAsync(BankCard card)
    {
        const string query =
            "INSERT INTO Cards(Name, Lastname, CardNumber, ExpirationDate, CVV, PinCode, PersonId) VALUES (@Name, @Lastname, @CardNumber, @ExpirationDate, @CVV, @PinCode, @PersonId)";

        await _connection.ExecuteAsync(query, card, _transaction);
    }
}
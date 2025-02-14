using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

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
            "INSERT INTO BankCards(FirstName, Lastname, CardNumber, ExpirationDate, CVV, PinCode, AccountId) VALUES (@FirstName, @Lastname, @CardNumber, @ExpirationDate, @CVV, @PinCode, @AccountId)";

        await _connection.ExecuteAsync(query, card, _transaction);
    }

    public async Task<BankCard?> GetCardAsync(string cardNumber)
    {
        return await _connection.QuerySingleOrDefaultAsync<BankCard>(
            "select * from BankCards where cardNumber = @CardNumber", new
            {
                CardNumber = cardNumber
            });
    }
    public async Task<bool> ValidateCardAsync(string cardNumber, string pinCode)
    {
        var card = await GetCardAsync(cardNumber);
        return card != null && pinCode == card.PinCode;
    }
}
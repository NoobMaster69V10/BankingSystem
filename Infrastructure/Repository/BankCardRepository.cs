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
        return await _connection.QueryFirstOrDefaultAsync<BankCard>(
            "select * from BankCards where CardNumber = @CardNumber", new
            {
                CardNumber = cardNumber
            });
    }
    public async Task<bool> ValidateCardAsync(string cardNumber, string pinCode)
    {
        var card = await GetCardAsync(cardNumber);
        return card != null && pinCode == card.PinCode;
    }

    public async Task UpdatePinAsync(string cardNumber, string pinCode)
    {
        var card = await GetCardAsync(cardNumber);
    
        if (card == null)
        {
            throw new KeyNotFoundException("Card not found.");
        }

        const string query = @"
        UPDATE BankCards 
        SET PinCode = @PinCode 
        WHERE CardNumber = @CardNumber";

        await _connection.ExecuteAsync(query, new { PinCode = pinCode, CardNumber = cardNumber }, _transaction);        
    }
    
    public async Task<decimal> GetBalanceAsync(string cardNumber)
    {
        const string query = """
                             
                                     SELECT ba.Balance 
                                     FROM BankAccounts ba
                                     INNER JOIN BankCards bc ON bc.AccountId = ba.Id
                                     WHERE bc.CardNumber = @CardNumber
                             """;

        return await _connection.QuerySingleOrDefaultAsync<decimal>(query, new { CardNumber = cardNumber });    }

    public async Task<BankAccount?> GetAccountAsync(string cardNumber)
    {
        return await _connection.QuerySingleOrDefaultAsync<BankAccount>(
            "SELECT b.* FROM BankCards bc INNER JOIN BankAccounts b ON bc.AccountId = b.Id WHERE bc.CardNumber = @CardNumber", 
            new { CardNumber = cardNumber });
    }

}
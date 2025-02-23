using System.Data;
using BankingSystem.Core.Helpers;
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
            "INSERT INTO BankCards(FirstName, Lastname, CardNumber, ExpirationDate, CVV, PinCode, Salt, AccountId) VALUES (@FirstName, @Lastname, @CardNumber, @ExpirationDate, @CVV, @PinCode, @Salt, @AccountId)";

        await _connection.ExecuteAsync(query, card, _transaction);
    }

    public async Task<BankCard?> GetCardAsync(string cardNumber)
    {
        return await _connection.QueryFirstOrDefaultAsync<BankCard>(
            "select * from BankCards where CardNumber = @CardNumber", 
            new { CardNumber = cardNumber },
            _transaction);
    }

    public async Task<bool> DoesCardExistAsync(string cardNumber)
    {
        string query = "SELECT CASE WHEN EXISTS (SELECT 1 FROM BankCards WHERE CardNumber = @CardNumber) THEN 1 ELSE 0 END";
        return await _connection.ExecuteScalarAsync<bool>(query, new { CardNumber = cardNumber }, _transaction);
    }

    public async Task<bool> IsCardExpiredAsync(string cardNumber)
    {
        string query =
            "SELECT CASE WHEN ExpirationDate < GETDATE() THEN 1 ELSE 0 END FROM BankCards WHERE CardNumber = @CardNumber";

        return await _connection.ExecuteScalarAsync<bool>(query, new { CardNumber = cardNumber }, _transaction);
    }

    public async Task<bool> CheckPinCodeAsync(string cardNumber, string pinCode)
    {
        string query =
            "SELECT PinCode, Salt FROM BankCards WHERE CardNumber = @CardNumber";

        var result = await _connection.QuerySingleOrDefaultAsync<(string PinCode, string Salt)>(query, new { CardNumber = cardNumber }, _transaction);

        return HashingHelper.VerifyHash(pinCode, result.PinCode, result.Salt);
    }

    public async Task UpdatePinAsync(string cardNumber, string pinCode)
    { 
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

        return await _connection.QuerySingleOrDefaultAsync<decimal>(query, new { CardNumber = cardNumber }, _transaction);
    }

    public async Task<BankAccount?> GetAccountByCardAsync(string cardNumber)
    {
        return await _connection.QuerySingleOrDefaultAsync<BankAccount>(
            "SELECT b.Id as BankAccountId FROM BankCards bc INNER JOIN BankAccounts b ON bc.AccountId = b.Id WHERE bc.CardNumber = @CardNumber",
            new { CardNumber = cardNumber }, _transaction);
    }
}
﻿using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class BankCardRepository : RepositoryBase, IBankCardRepository
{
    public BankCardRepository(IDbConnection connection) : base(connection) { }

    public async Task<BankCard?> GetCardAsync(string cardNumber)
    {
        return await Connection.QueryFirstOrDefaultAsync<BankCard>(
            "select * from BankCards where CardNumber = @CardNumber",
            new { CardNumber = cardNumber },
            transaction: Transaction);
    }

    public async Task<bool> DoesCardExistAsync(string cardNumber)
    {
        string query = "SELECT CASE WHEN EXISTS (SELECT 1 FROM BankCards WHERE CardNumber = @CardNumber) THEN 1 ELSE 0 END";
        return await Connection.ExecuteScalarAsync<bool>(query, new { CardNumber = cardNumber }, Transaction);
    }

    public async Task<bool> IsCardExpiredAsync(string cardNumber)
    {
        string query =
            "SELECT CASE WHEN ExpirationDate < GETDATE() THEN 1 ELSE 0 END FROM BankCards WHERE CardNumber = @CardNumber";

        return await Connection.ExecuteScalarAsync<bool>(query, new { CardNumber = cardNumber }, Transaction);
    }

    public async Task<(string PinCode,DateTime ExpiryDate, string Cvv)?> GetCardDetailsAsync(string cardNumber)
    {
        const string query = @"
        SELECT PinCode,ExpirationDate, CVV
        FROM BankCards
        WHERE CardNumber = @CardNumber";

        return await Connection.QuerySingleOrDefaultAsync<(string, DateTime, string)>(
            query, new { CardNumber = cardNumber }, Transaction);
    }

    public async Task AddCardAsync(BankCard card)
    {
        const string query = "INSERT INTO BankCards (CardNumber,PinCode, ExpirationDate, CVV, AccountId) VALUES (@CardNumber, @PinCode, @ExpirationDate, @CVV, @AccountId)";
        await Connection.ExecuteAsync(query, card, Transaction);
    }

    public async Task UpdatePinAsync(string cardNumber, string pinCode)
    {
        const string query = @"
        UPDATE BankCards 
        SET PinCode = @PinCode
        WHERE CardNumber = @CardNumber";

        await Connection.ExecuteAsync(query, new { PinCode = pinCode, CardNumber = cardNumber}, Transaction);
    }

    public async Task<decimal> GetBalanceAsync(string cardNumber)
    {
        const string query = """
                                     SELECT ba.Balance 
                                     FROM BankAccounts ba
                                     INNER JOIN BankCards bc ON bc.AccountId = ba.BankAccountId
                                     WHERE bc.CardNumber = @CardNumber
                             """;

        return await Connection.QuerySingleOrDefaultAsync<decimal>(query, new { CardNumber = cardNumber }, Transaction);
    }

    public async Task<BankAccount?> GetAccountByCardAsync(string cardNumber)
    {
        return await Connection.QuerySingleOrDefaultAsync<BankAccount>(
            @"SELECT b.BankAccountId, Currency, IBAN, Balance 
                 FROM BankCards bc INNER JOIN BankAccounts b ON bc.AccountId = b.BankAccountId 
                 WHERE bc.CardNumber = @CardNumber",
            new { CardNumber = cardNumber }, Transaction);
    }
}

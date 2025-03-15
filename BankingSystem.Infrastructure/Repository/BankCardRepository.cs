using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class  BankCardRepository : RepositoryBase, IBankCardRepository
{
    public BankCardRepository(IDbConnection connection) : base(connection) { }

    public async Task<BankCard?> GetCardAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM BankCards WHERE CardNumber = @CardNumber AND IsActive = 1";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.QueryFirstOrDefaultAsync<BankCard>(parameters);
    }

    public async Task<string?> GetCardIdAsync(string cardNumber,CancellationToken cancellationToken = default)
    {
        const string query = "SELECT BankCardId FROM BankCards Where @CardNumber = CardNumber and IsActive = 1";
        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);
        return await Connection.QueryFirstOrDefaultAsync<string>(parameters);
    } 
    
    public async Task<bool> DoesCardExistAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT CASE WHEN EXISTS (SELECT 1 FROM BankCards WHERE CardNumber = @CardNumber) THEN 1 ELSE 0 END";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.ExecuteScalarAsync<bool>(parameters);
    }

    public async Task<bool> IsCardExpiredAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT CASE WHEN ExpirationDate < GETDATE() THEN 1 ELSE 0 END FROM BankCards WHERE CardNumber = @CardNumber";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.ExecuteScalarAsync<bool>(parameters);
    }

    public async Task<(string PinCode,DateTime ExpiryDate, string Cvv)?> GetCardSecurityDetailsAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT PinCode,ExpirationDate, CVV
        FROM BankCards
        WHERE CardNumber = @CardNumber AND IsActive = 1";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.QuerySingleOrDefaultAsync<(string, DateTime, string)>(parameters);
    }

    public async Task AddCardAsync(BankCard card, CancellationToken cancellationToken = default)
    {
        const string query = "INSERT INTO BankCards (CardNumber,PinCode, ExpirationDate, CVV, AccountId) VALUES (@CardNumber, @PinCode, @ExpirationDate, @CVV, @AccountId)";
        
        var parameters = new CommandDefinition(query, card, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }

    public async Task UpdatePinAsync(string cardNumber, string pinCode, CancellationToken cancellationToken = default)
    {
        const string query = @"
        UPDATE BankCards 
        SET PinCode = @PinCode
        WHERE CardNumber = @CardNumber";

        var parameters = new CommandDefinition(query, new { PinCode = pinCode, CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }

    public async Task<decimal> GetBalanceAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = """
                                     SELECT ba.Balance 
                                     FROM BankAccounts ba
                                     INNER JOIN BankCards bc ON bc.AccountId = ba.BankAccountId
                                     WHERE bc.CardNumber = @CardNumber
                             """;

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.QuerySingleOrDefaultAsync<decimal>(parameters);
    }

    public async Task<BankAccount?> GetAccountByCardAsync(string cardNumber, CancellationToken cancellationToken)
    {
        const string query = @"SELECT b.BankAccountId, Currency, IBAN, Balance,PersonId 
                 FROM BankCards bc INNER JOIN BankAccounts b ON bc.AccountId = b.BankAccountId 
                 WHERE bc.CardNumber = @CardNumber AND bc.IsActive = 1";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.QuerySingleOrDefaultAsync<BankAccount>(parameters);
    }
    public async Task RemoveBankCardAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = "DELETE FROM BankCards WHERE CardNumber = @CardNumber";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }
    public async Task DeactivateCardAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        const string query = "UPDATE BankCards SET IsActive = 0 WHERE CardNumber = @CardNumber";

        var parameters = new CommandDefinition(query, new { CardNumber = cardNumber }, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }
}

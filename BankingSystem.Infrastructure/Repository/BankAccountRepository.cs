using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class BankAccountRepository : RepositoryBase, IBankAccountRepository
{
    public BankAccountRepository(IDbConnection connection) : base(connection) { }

    public async Task AddBankAccountAsync(BankAccount account, CancellationToken cancellationToken = default)
    {
        const string query = "INSERT INTO BankAccounts (IBAN, Balance, Currency, PersonId) VALUES (@Iban, @Balance, @Currency, @PersonId)";

        var parameters = new CommandDefinition(query, account, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }

    public async Task<BankAccount?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM BankAccounts WHERE BankAccountId = @BankAccountId AND IsActive = 1";

        var parameters = new CommandDefinition(query, new { BankAccountId = accountId }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.QueryFirstOrDefaultAsync<BankAccount?>(parameters);
    }

    public async Task UpdateBalanceAsync(BankAccount? account, CancellationToken cancellationToken = default)
    {
        const string query = "UPDATE BankAccounts SET Balance = @Balance WHERE BankAccountId = @BankAccountId";

        var parameters = new CommandDefinition(query, account, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }

    public async Task<BankAccount?> GetAccountByIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM BankAccounts WHERE IBAN = @IBAN And IsActive = 1";
        var parameters = new CommandDefinition(query, new { IBAN = iban }, cancellationToken: cancellationToken, transaction: Transaction);

        return await Connection.QueryFirstOrDefaultAsync<BankAccount?>(parameters);
    }

    public async Task RemoveBankAccountAsync(string iban, CancellationToken cancellationToken = default)
    {
        const string query = "UPDATE BankAccounts SET IsActive = 0 WHERE IBAN = @IBAN";
        var parameters = new CommandDefinition(query, new { IBAN = iban }, cancellationToken: cancellationToken, transaction: Transaction);
        await Connection.ExecuteAsync(parameters);
    }
}

using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class BankAccountRepository : TransactionalRepositoryBase, IBankAccountRepository
{
    public BankAccountRepository(IDbConnection connection) : base(connection) { }

    public async Task AddBankAccountAsync(BankAccount account)
    {
        const string query = "INSERT INTO BankAccounts (IBAN, Balance, Currency, PersonId) VALUES (@Iban, @Balance, @Currency, @PersonId)";
        await Connection.ExecuteAsync(query, new{ account.Iban, account.Balance, Currency = account.Currency.ToString(), account.PersonId}, Transaction);
    }

    public async Task<BankAccount?> GetAccountByIdAsync(int accountId)
    {
        const string query = "SELECT * FROM BankAccounts WHERE BankAccountId = @BankAccountId";
        return await Connection.QueryFirstOrDefaultAsync<BankAccount?>(query, new { BankAccountId = accountId }, Transaction);
    }

    public async Task UpdateBalanceAsync(BankAccount? account)
    {
        const string query = "UPDATE BankAccounts SET Balance = @Balance WHERE BankAccountId = @BankAccountId";
        await Connection.ExecuteAsync(query, account, Transaction);
    }

    public async Task<BankAccount?> GetAccountByIbanAsync(string iban)
    {
        return await Connection.QueryFirstOrDefaultAsync<BankAccount?>(
            "select * from BankAccounts where IBAN = @iban", new
            {
                IBAN = iban 
            });
    }
    public async Task<string> GetAccountCurrencyAsync(int accountId)
    {
        const string query = "SELECT Currency FROM BankAccounts WHERE BankAccountId = @AccountId";
        var result = await Connection.QueryFirstOrDefaultAsync<string>(query, new { AccountId = accountId });
        return result!;
    }
}

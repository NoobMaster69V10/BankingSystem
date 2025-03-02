using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(IDbConnection connection) : base(connection, "BankAccounts") { }

    public Task UpdateBalanceAsync(BankAccount? account, decimal balance)
    {
        const string query = "UPDATE BankAccounts SET Balance = @Balance WHERE BankAccountId = @Id";
        return Connection.ExecuteAsync(query, new { Id = account.BankAccountId, Balance = balance }, Transaction);
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
        return await Connection.QueryFirstOrDefaultAsync<string>(query, new { AccountId = accountId });
    }
}

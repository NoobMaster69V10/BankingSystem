using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BankingSystem.Infrastructure.Data.Repository;

public class AccountRepository(SqlConnection conn) : IAccountRepository
{
    public async Task CreateAccountAsync(BankAccount account)
    {
        const string query = "INSERT INTO BankAccounts(IBAN, Balance, Currency, UserId) VALUES (@IBAN, @Balance, @Currency, @UserId)";

        await conn.ExecuteAsync(query, new { IBAN = account.IBAN, Balance = account.Balance, Currency = account.Currency, UserId = account.UserId});
    }

    public async Task UpdateAccountAsync(BankAccount account, SqlTransaction transaction)
    {
        const string query = "UPDATE BankAccounts SET IBAN = @IBAN, Balance = @Balance, Currency = @Currency, UserId = @UserId WHERE Id = @Id";

        await conn.ExecuteAsync(query, account, transaction);
    }

    public async Task<BankAccount> GetAccountByIdAsync(int id, SqlTransaction transaction)
    {
        const string query = "SELECT * FROM BankAccounts WHERE Id = @Id";

        return await conn.QueryFirstAsync<BankAccount>(query, new { Id = id }, transaction);
    }
}
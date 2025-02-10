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
}
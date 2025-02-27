using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BankingSystem.Infrastructure.Repository;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly SqlConnection _connection;
    private IDbTransaction _transaction;

    public BankAccountRepository(SqlConnection connection,IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }
    public async Task CreateAccountAsync(BankAccount account)
    {
        const string query = "INSERT INTO BankAccounts(IBAN, Balance, Currency, PersonId) VALUES (@IBAN, @Balance, @Currency, @PersonId)";

        await _connection.ExecuteAsync(query, account, _transaction);
    }

    public async Task UpdateAccountAsync(BankAccount account)
    {
        const string query = "UPDATE BankAccounts SET IBAN = @IBAN, Balance = @Balance, Currency = @Currency, PersonId= @PersonId WHERE Id = @BankAccountId";

        await _connection.ExecuteAsync(query, account, _transaction);
    }

    public async Task<BankAccount?> GetAccountByIdAsync(int id)
    {
        const string query = "SELECT Id AS BankAccountId, IBAN, Balance, Currency, PersonId FROM BankAccounts WHERE Id = @Id";

        return await _connection.QueryFirstOrDefaultAsync<BankAccount>(query, new { Id = id });
    }
    
    public Task UpdateBalanceAsync(BankAccount? account, decimal balance)
    {
        const string query = "UPDATE BankAccounts SET Balance = @Balance WHERE Id = @Id";
        return _connection.ExecuteAsync(query,new {Id = account.BankAccountId,Balance = balance}, _transaction);
    }

    public async Task<BankAccount?> GetAccountByIbanAsync(string iban)
    {
        return await _connection.QueryFirstOrDefaultAsync<BankAccount?>(
            "select * from BankAccounts where IBAN = @iban", new
            {
                    IBAN = iban
            });
    }
}
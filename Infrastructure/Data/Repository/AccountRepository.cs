﻿using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Data.Repository;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;

    public AccountRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CreateAccountAsync(BankAccount account)
    {
        const string query = "INSERT INTO BankAccounts(IBAN, Balance, Currency, PersonId) VALUES (@IBAN, @Balance, @Currency, @PersonId)";

        await _connection.ExecuteAsync(query, account, _transaction);
    }

    public async Task UpdateAccountAsync(BankAccount account)
    {
        const string query = "UPDATE BankAccounts SET IBAN = @IBAN, Balance = @Balance, Currency = @Currency,  PersonId= @PersonId WHERE Id = @Id";

        await _connection.ExecuteAsync(query, account, _transaction);
    }

    public async Task<BankAccount> GetAccountByIdAsync(int id)
    {
        const string query = "SELECT * FROM BankAccounts WHERE Id = @Id";

        return await _connection.QueryFirstAsync<BankAccount>(query, new { Id = id }, _transaction);
    }
}
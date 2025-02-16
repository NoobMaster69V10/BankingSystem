﻿using System.Data;
using BankingSystem.Core.DTO;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class TransactionRepository : IAccountTransactionRepository
{
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;

    public TransactionRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task AddAccountTransactionAsync(AccountTransaction transactionObj)
    {
        const string query =
            "INSERT INTO AccountTransactions(Amount, Currency, TransactionDate, FromAccountId, ToAccountId) VALUES (@Amount, @Currency, @TransactionDate, @FromAccountId, @ToAccountId)";

        await _connection.ExecuteAsync(query, transactionObj, _transaction);
    }

    public async Task AddAtmTransactionAsync(AtmTransaction atmTransaction)
    {
        const string query =
            "Insert into ATMWithdrawals (Amount,Currency,TransactionDate,AccountId) VALUES (@Amount,@Currency,@TransactionDate,@AccountId)";
        await _connection.ExecuteAsync(query, atmTransaction, _transaction);
    }
}
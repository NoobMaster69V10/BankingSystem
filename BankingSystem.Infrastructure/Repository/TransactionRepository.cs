using System.Data;
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
            "INSERT INTO AccountTransactions(Amount, Currency, TransactionDate, FromAccountId, ToAccountId, TransactionFee) VALUES (@Amount, @Currency, @TransactionDate, @FromAccountId, @ToAccountId, @TransactionFee)";

        await _connection.ExecuteAsync(query, transactionObj, _transaction);
    }

    public async Task AddAtmTransactionAsync(AtmTransaction atmTransaction)
    {
        const string query =
            "Insert into ATMWithdrawals (Amount,Currency,TransactionDate,AccountId,TransactionFee) VALUES (@Amount,@Currency,@TransactionDate,@AccountId,@TransactionFee)";
        await _connection.ExecuteAsync(query, atmTransaction, _transaction);
    }

    public async Task<int> GetTotalWithdrawnTodayAsync(int accountId)
    {
        const string query = @"
        SELECT COALESCE(SUM(Amount), 0)
        FROM ATMWithdrawals
        WHERE AccountId = @AccountId AND CAST(TransactionDate AS DATE) = CAST(GETDATE() AS DATE)";
    
        return await _connection.QueryFirstOrDefaultAsync<int>(query, new { AccountId = accountId },_transaction);
    }
}

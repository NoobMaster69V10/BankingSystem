using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class BankTransactionRepository : RepositoryBase, IBankTransactionRepository
{
    public BankTransactionRepository(IDbConnection connection) : base(connection)
    {
    }

    public async Task AddAccountTransferAsync(AccountTransfer transferObj)
    {
        const string query =
            "INSERT INTO AccountTransactions(Amount, TransactionDate, FromAccountId, ToAccountId, TransactionFee, TransactionType) VALUES (@Amount, @TransactionDate, @FromAccountId, @ToAccountId, @TransactionFee, @TransactionType)";

        await Connection.ExecuteAsync(query, transferObj, Transaction);
    }

    public async Task<decimal> GetTotalWithdrawnTodayAsync(int accountId)
    {
        const string query = @"
                SELECT COALESCE(SUM(Amount), 0) 
                FROM AccountTransactions 
                WHERE FromAccountId = @AccountId
                AND TransactionType = @TransactionType
                AND TransactionDate >= @TransactionDate";

        var parameters = new
        {
            AccountId = accountId,
            TransactionDate = DateTime.Now.AddHours(-24),
            TransactionType = TransactionType.Atm
        };
        
        return await Connection.QueryFirstOrDefaultAsync<decimal>(query, parameters, Transaction);
    }

    public async Task AddAtmTransactionAsync(AtmTransaction atmTransaction)
    {
        const string query =
            "INSERT INTO AccountTransactions(Amount, TransactionDate, FromAccountId, TransactionFee, TransactionType) VALUES (@Amount, @TransactionDate, @AccountId, @TransactionFee, @TransactionType)";

        await Connection.ExecuteAsync(query, atmTransaction);
    }
}
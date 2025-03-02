using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class AtmRepository : GenericRepository<AtmTransaction>, IAtmRepository
{
    public AtmRepository(IDbConnection connection) : base(connection, "AtmTransactions")
    {
    }

    public async Task<decimal> GetTotalWithdrawnTodayAsync(int accountId)
    {
        const string query = @"
    SELECT COALESCE(SUM(Amount), 0) 
    FROM AtmTransactions
    WHERE AccountId = @AccountId
    AND CAST(TransactionDate AS DATE) = @TransactionDate";

        var parameters = new { AccountId = accountId, TransactionDate = DateTime.UtcNow.Date };

        return await Connection.QueryFirstOrDefaultAsync<decimal>(query, parameters, Transaction);
    }

    public async Task AddAtmTransactionAsync(AtmTransaction atmTransaction)
    {
        const string query =
            "Insert into AtmTransactions (Amount,TransactionDate,AccountId,TransactionFee) VALUES (@Amount,@TransactionDate,@AccountId,@TransactionFee)";
        await Connection.ExecuteAsync(query, atmTransaction, Transaction);
    }
    public async Task<IEnumerable<AtmTransaction>> GetAllAtmTransactionsAsync()
    {
        const string query = @"
        SELECT at.Amount, ba.Currency 
        FROM AtmTransactions at
        JOIN BankAccounts ba ON at.AccountId = ba.BankAccountId";

        return await Connection.QueryAsync<AtmTransaction>(query);
    }

}
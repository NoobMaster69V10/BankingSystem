using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class AccountTransactionRepository : GenericRepository<AccountTransaction>, IAccountTransactionRepository
{
    public AccountTransactionRepository(IDbConnection connection) : base(connection, "AccountTransactions") { }

    public async Task AddAtmTransactionAsync(AtmTransaction atmTransaction)
    {
        const string query =
            "Insert into ATMWithdrawals (Amount,Currency,TransactionDate,AccountId,TransactionFee) VALUES (@Amount,@Currency,@TransactionDate,@AccountId,@TransactionFee)";
        await Connection.ExecuteAsync(query, atmTransaction, Transaction);
    }

    public async Task<int> GetTotalWithdrawnTodayAsync(int accountId)
    {
        const string query = @"
        SELECT COALESCE(SUM(Amount), 0)
        FROM ATMWithdrawals
        WHERE AccountId = @AccountId AND CAST(TransactionDate AS DATE) = CAST(GETDATE() AS DATE)";

        return await Connection.QueryFirstOrDefaultAsync<int>(query, new { AccountId = accountId }, Transaction);
    }

}

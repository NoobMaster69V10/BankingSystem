using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class AtmRepository : GenericRepository<AtmTransaction>, IAtmRepository
{
    public AtmRepository(IDbConnection connection, string tableName) : base(connection, tableName)
    {
    }
    public async Task<int> GetTotalWithdrawnTodayAsync(int accountId)
    {
        const string query = @"
        SELECT COALESCE(SUM(Amount), 0)
        FROM ATMWithdrawals
        WHERE AccountId = @AccountId 
        AND CAST(TransactionDate AS DATE) = @TransactionDate";

        var parameters = new { AccountId = accountId, TransactionDate = DateTime.UtcNow.Date };

        return await Connection.QueryFirstOrDefaultAsync<int>(query, parameters, Transaction);
    }
}
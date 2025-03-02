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
}

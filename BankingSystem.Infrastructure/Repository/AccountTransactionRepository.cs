using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class AccountTransactionRepository : GenericRepository<AccountTransaction>, IAccountTransactionRepository
{
    public AccountTransactionRepository(IDbConnection connection) : base(connection, "AccountTransactions") { }

    public async Task AddAccountTransactionAsync(AccountTransaction transactionObj)
    {
        const string query =
            "INSERT INTO AccountTransactions(Amount, Currency, TransactionDate, FromAccountId, ToAccountId, TransactionFee) VALUES (@Amount, @Currency, @TransactionDate, @FromAccountId, @ToAccountId, @TransactionFee)";

        await Connection.ExecuteAsync(query, transactionObj, Transaction);
    }
}

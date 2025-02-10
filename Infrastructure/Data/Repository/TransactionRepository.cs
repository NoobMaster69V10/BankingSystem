using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BankingSystem.Infrastructure.Data.Repository;

public class TransactionRepository(SqlConnection conn, IAccountRepository accountRepo) : ITransactionRepository
{
    public async Task TransactionBetweenOwnAccountsAsync(Transaction transactionObj)
    {
        await conn.OpenAsync();
        await using var transaction = conn.BeginTransaction();

        try
        {
            var fromAccount = await accountRepo.GetAccountByIdAsync(transactionObj.FromAccountId, transaction);
            var toAccount = await accountRepo.GetAccountByIdAsync(transactionObj.ToAccountId, transaction);

            fromAccount.Balance -= transactionObj.Amount;
            toAccount.Balance += transactionObj.Amount;

            await accountRepo.UpdateAccountAsync(fromAccount, transaction);
            await accountRepo.UpdateAccountAsync(toAccount, transaction);

            const string query =
                "INSERT INTO Transactions(Amount, Currency, TransactionDate, FromAccountId, ToAccountId) VALUES (@Amount, @Currency, @TransactionDate, @FromAccountId, @ToAccountId)";

            await conn.ExecuteAsync(query, transactionObj, transaction);

            await transaction.CommitAsync();
            await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex);
        }

    }

    public Task TransactionBetweenAnotherBankAccountsAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }
}
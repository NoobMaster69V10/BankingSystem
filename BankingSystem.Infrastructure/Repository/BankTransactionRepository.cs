using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class BankTransactionRepository : RepositoryBase, IBankTransactionRepository
{
    public BankTransactionRepository(IDbConnection connection) : base(connection) { }

    public async Task TransferBetweenAccountsAsync(AccountTransfer accountTransfer, decimal convertedAmount, CancellationToken cancellationToken = default)
    {

        var parameters = new DynamicParameters();
        parameters.Add("@FromAccountId", accountTransfer.FromAccountId, DbType.Int32);
        parameters.Add("@ToAccountId", accountTransfer.ToAccountId, DbType.Int32);
        parameters.Add("@Amount", accountTransfer.Amount, DbType.Decimal);
        parameters.Add("@TransactionFee", accountTransfer.TransactionFee, DbType.Decimal);
        parameters.Add("@ConvertedAmount", convertedAmount, DbType.Decimal);

        var command = new CommandDefinition("TransferBetweenAccounts", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        await Connection.ExecuteAsync(command);
    }

    public async Task<decimal> GetTotalWithdrawnTodayAsync(int accountId, CancellationToken cancellationToken = default)
    {
        const string query = @"
                SELECT COALESCE(SUM(Amount), 0) 
                FROM AccountTransactions 
                WHERE FromAccountId = @AccountId
                AND TransactionType = @TransactionType
                AND TransactionDate >= @TransactionDate";

        var parameters = new CommandDefinition(query, new { AccountId = accountId, TransactionDate = DateTime.Now.AddHours(-24), TransactionType = TransactionType.Atm }, cancellationToken: cancellationToken, transaction: Transaction);
        
        return await Connection.QueryFirstOrDefaultAsync<decimal>(parameters);
    }

    public async Task AddAtmTransactionAsync(AtmTransaction atmTransaction, CancellationToken cancellationToken = default)
    {
        const string query =
            "INSERT INTO AccountTransactions(Amount, TransactionDate, FromAccountId, TransactionFee, TransactionType) VALUES (@Amount, @TransactionDate, @AccountId, @TransactionFee, @TransactionType)";

        var parameters = new CommandDefinition(query, atmTransaction, cancellationToken: cancellationToken, transaction: Transaction);

        await Connection.ExecuteAsync(parameters);
    }
}
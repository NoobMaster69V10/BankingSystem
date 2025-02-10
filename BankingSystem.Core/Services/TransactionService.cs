using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;
using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;

namespace BankingSystem.Core.Services;

public class TransactionService(ITransactionRepository transactionRepo) : ITransactionService
{
    public async Task TransactionBetweenOwnAccountsAsync(TransactionDto transactionDto)
    {
        var transaction = new Transaction {
            FromAccountId = transactionDto.FromAccountId,
            ToAccountId = transactionDto.ToAccountId,
            Currency = transactionDto.Currency,
            Amount = transactionDto.Amount,
            TransactionDate = transactionDto.TransactionDate
        };

        await transactionRepo.TransactionBetweenOwnAccountsAsync(transaction);

    }

    public Task TransactionBetweenAnotherBankAccountsAsync(TransactionDto transactionDto)
    {
        throw new NotImplementedException();
    }
}
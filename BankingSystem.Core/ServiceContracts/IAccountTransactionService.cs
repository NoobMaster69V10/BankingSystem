using BankingSystem.Core.DTO;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<string> TransactionBetweenAccountsAsync(TransactionDto transactionDto);
}
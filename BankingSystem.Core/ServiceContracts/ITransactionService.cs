using BankingSystem.Core.DTO;

namespace BankingSystem.Core.ServiceContracts;

public interface ITransactionService
{
    Task TransactionBetweenOwnAccountsAsync(TransactionDto transactionDto);
    Task TransactionBetweenAnotherBankAccountsAsync(TransactionDto transactionDto);
}
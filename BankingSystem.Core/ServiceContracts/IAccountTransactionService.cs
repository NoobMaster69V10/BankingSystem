using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<Result<AccountTransfer>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId, CancellationToken cancellationToken = default); 
}
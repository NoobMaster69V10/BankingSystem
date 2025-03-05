using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<Result<AccountTransfer>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId); 
    
}
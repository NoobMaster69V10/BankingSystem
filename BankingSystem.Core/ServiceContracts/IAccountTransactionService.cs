using System.Security.AccessControl;
using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<Result<AccountTransaction>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId); 
    
}
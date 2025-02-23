using System.Security.AccessControl;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<CustomResult<AccountTransaction>> TransactionBetweenAccountsAsync(TransactionDto transactionDto, string userId);
    Task<CustomResult<bool>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto);
}
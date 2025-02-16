using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<ApiResponse> TransactionBetweenAccountsAsync(TransactionDto transactionDto, string userId);
    Task<ApiResponse> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto);
}
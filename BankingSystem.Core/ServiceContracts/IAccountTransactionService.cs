﻿using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountTransactionService
{
    Task<AdvancedApiResponse<string>> TransactionBetweenAccountsAsync(TransactionDto transactionDto, string userId);
    Task<ApiResponse> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto);
}
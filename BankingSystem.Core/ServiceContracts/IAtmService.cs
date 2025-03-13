using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<Result<BalanceResponse>> ShowBalanceAsync(CardAuthorizationDto cardDto);
    Task<Result<string>> ChangePinAsync(ChangePinDto pinDto);
    Task<Result<AtmTransactionResponse>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto);
    Task<Result<BalanceResponse>> DepositMoneyAsync(DepositMoneyDto cardDto);
}
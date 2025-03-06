using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using Sprache;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<Result<BalanceResponse>> ShowBalanceAsync(CardAuthorizationDto cardDto);
    Task<Result<string>> ChangePinAsync(ChangePinDto pinDto);
    Task<Result<AtmTransactionResponse>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto);
    Task<Result<BalanceResponse>> DepositMoneyAsync(DepositMoneyDto cardDto);
}
using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<Result<BalanceResponse>> ShowBalanceAsync(CardAuthorizationDto cardDto, CancellationToken cancellationToken = default);
    Task<Result<string>> ChangePinAsync(ChangePinDto pinDto, CancellationToken cancellationToken = default);
    Task<Result<AtmTransactionResponse>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto, CancellationToken cancellationToken = default);
    Task<Result<BalanceResponse>> DepositMoneyAsync(DepositMoneyDto cardDto, CancellationToken cancellationToken = default);
}
using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<Result<BalanceResponseDto>> ShowBalanceAsync(CardAuthorizationDto cardDto);
    Task<Result<bool>> ChangePinAsync(ChangePinDto pinDto);
}
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<CustomResult<BalanceResponseDto>> ShowBalanceAsync(CardAuthorizationDto cardDto);
    Task<CustomResult<bool>> ChangePinAsync(ChangePinDto pinDto);
}
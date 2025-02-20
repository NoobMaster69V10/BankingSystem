using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<ApiResponse> ShowBalanceAsync(CardAuthorizationDto cardDto);
    Task<ApiResponse> ChangePinAsync(ChangePinDto pinDto);
}
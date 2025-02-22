using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IAtmService
{
    Task<ResultT<decimal>> ShowBalanceAsync(CardAuthorizationDto cardDto);
    Task<Result> ChangePinAsync(ChangePinDto pinDto);
}
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AtmService : IAtmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankCardService _bankCardService;
    private readonly ILoggerService _loggerService;

    public AtmService(IUnitOfWork unitOfWork, IBankCardService bankCardService, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _bankCardService = bankCardService;
        _loggerService = loggerService;
    }

    public async Task<Result> AuthorizeCardAsync(string cardNumber, string pin)
    {
        try
        {
            var validationResult = await _bankCardService.ValidateCardAsync(cardNumber, pin);
            if (!validationResult.IsSuccess)
            {
                return Result.Failure(validationResult.Error);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in AuthorizeCardAsync: {ex}");
            return Error.Failure("Error occured while authorize");
        }
    }

    public async Task<ResultT<decimal>> ShowBalanceAsync(CardAuthorizationDto cardDto)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode);
            if (!authResult.IsSuccess)
            {
                return ResultT<decimal>.Failure(authResult.Error);
            }

            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber);
            return ResultT<decimal>.Success(balance);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ShowBalanceAsync: {ex}");
            return ResultT<decimal>.Failure(
                Error.Failure(
                    "An error occurred while retrieving the balance"
                ));
            
        }
    }

    public async Task<Result> ChangePinAsync(ChangePinDto changePinDto)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin);
            if (!authResult.IsSuccess)
            {
                return Result.Failure(authResult.Error);
            }

            await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CurrentPin, changePinDto.NewPin);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ChangePinAsync: {ex}");
            return Error.Failure(
                "An error occurred while changing the PIN"
            );
        }
    }
}

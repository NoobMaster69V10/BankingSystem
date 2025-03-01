using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
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


    public async Task<CustomResult<BalanceResponseDto>> ShowBalanceAsync(CardAuthorizationDto cardDto)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode);
            if (authResult.IsFailure)
            {
                if (authResult.Error != null) return CustomResult<BalanceResponseDto>.Failure(authResult.Error);
            }

            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber);
            var response = new BalanceResponseDto(
                balance: balance,
                cardNumber: cardDto.CardNumber
            );

            return CustomResult<BalanceResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ShowBalanceAsync: {ex}");
            return CustomResult<BalanceResponseDto>.Failure(
                new CustomError("BALANCE_ERROR", "An error occurred while retrieving the balance")
            );
        }
    }

    public async Task<CustomResult<bool>> ChangePinAsync(ChangePinDto changePinDto)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin);
            if (!authResult.IsSuccess)
            {
                if (authResult.Error != null) return CustomResult<bool>.Failure(authResult.Error);
            }

            //await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.CurrentPin);
            return CustomResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ChangePinAsync: {ex}");
            return CustomResult<bool>.Failure(
                new CustomError("PIN_CHANGE_ERROR", "An error occurred while changing the PIN")
            );
        }
    }

    private async Task<CustomResult<bool>> AuthorizeCardAsync(string cardNumber, string pin)
    {
        try
        {
            var validationResult = await _bankCardService.ValidateCardAsync(cardNumber, pin);
            if (!validationResult.IsSuccess)
            {
                if (validationResult.Error != null) return CustomResult<bool>.Failure(validationResult.Error);
            }

            return CustomResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in AuthorizeCardAsync: {ex}");
            return CustomResult<bool>.Failure(new CustomError("AUTH_ERROR", "Error occurred while authorizing"));
        }
    }
}
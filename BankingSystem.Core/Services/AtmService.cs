using System.Net;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AtmService : IAtmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApiResponse _response;
    private readonly IBankCardService _bankCardService;
    private readonly ILoggerService _loggerService;


    public AtmService(IUnitOfWork unitOfWork, IBankCardService bankCardService, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _bankCardService = bankCardService;
        _loggerService = loggerService;
        _response = new();
    }

    public async Task<ApiResponse> AuthorizeCardAsync(string cardNumber, string pin)
    {
        try
        {
            return await _bankCardService.ValidateCardAsync(cardNumber, pin);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in CreateBankAccountAsync: {ex}");
            throw;
        }
    }
    public async Task<ApiResponse> ShowBalanceAsync(CardAuthorizationDto cardDto)
    {
        try
        {
            var response = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode);
            if (!response.IsSuccess)
            {
                return response;
            }
            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber);
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = new
            {
                Balance = balance
            };
            return _response;
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ShowBalanceAsync: {ex}");
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            return _response;
        }
    }

    public async Task<ApiResponse> ChangePinAsync(ChangePinDto changePinDto)
    {
        try
        {   
            var response = await AuthorizeCardAsync(changePinDto.CardNumber,changePinDto.CurrentPin);
            if (!response.IsSuccess)
            {
                return response;
            }
            await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CurrentPin, changePinDto.NewPin);
            _response.StatusCode = HttpStatusCode.Accepted;
            _response.Result = new
            {
                Message = "Success"
            };
            return _response;
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add(ex.Message);
            return _response;
        }
    }
}
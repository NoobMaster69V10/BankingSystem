using System.Net;
using System.Text.Unicode;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AtmService : IAtmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApiResponse _response;


    public AtmService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _response = new();
    }

    public async Task<ApiResponse> AuthorizeCardAsync(CardAuthorizationDto cardAuthorizationDto)
    {
        try
        {
            var authorized = await _unitOfWork.BankCardRepository.ValidateCardAsync(cardAuthorizationDto.CardNumber,
                cardAuthorizationDto.PinCode);
            if (!authorized)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Card number or pin code is invalid.");
                return _response;
            }

            _response.StatusCode = HttpStatusCode.NoContent;
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
    public async Task<ApiResponse> ShowBalanceAsync(string cardNumber)
    {
        try
        {
            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardNumber);
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = balance;
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


    public async Task<ApiResponse> ChangePinAsync(ChangePinDto changePinDto)
    {
        try
        {   
            var isValid = await _unitOfWork.BankCardRepository.ValidateCardAsync(changePinDto.CardNumber,changePinDto.CurrentPin);
            _response.StatusCode = HttpStatusCode.OK;
            if (!isValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Current PIN is incorrect");
                return _response;
            }
            await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.CurrentPin);
            _response.StatusCode = HttpStatusCode.OK;
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
    
    
    
    // public async Task<ApiResponse> ShowBalanceAsync(CardAuthorizationDto cardDto)
    // {
    //     try
    //     {
    //         var authorized = await _unitOfWork.BankCardRepository.ValidateCardAsync(cardDto.CardNumber,
    //             cardDto.PinCode);
    //         if (!authorized)
    //         {
    //             _response.StatusCode = HttpStatusCode.BadRequest;
    //             _response.IsSuccess = false;
    //             _response.ErrorMessages.Add("Card number or pin code is invalid.");
    //             return _response;
    //         }
    //
    //         var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber);
    //         _response.StatusCode = HttpStatusCode.OK;
    //         return _response;
    //     }
    //     catch (Exception ex)
    //     {
    //         _response.StatusCode = HttpStatusCode.BadRequest;
    //         _response.IsSuccess = false;
    //         _response.ErrorMessages.Add(ex.Message);
    //         return _response;
    //     }
    // }
}
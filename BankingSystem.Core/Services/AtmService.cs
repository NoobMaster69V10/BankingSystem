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
}
using System.Net;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<ApiResponse> ValidateCardAsync(string cardNumber, string pinCode)
    {
        if (!await unitOfWork.BankCardRepository.DoesCardExistAsync(cardNumber))
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccess = false,
                ErrorMessages = ["Card does not exist."]
            };
        }
        if (!await unitOfWork.BankCardRepository.CheckPinCodeAsync(cardNumber, pinCode))
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                IsSuccess = false,
                ErrorMessages = ["Pin code does not match."]
            };
        }

        if (await unitOfWork.BankCardRepository.IsCardExpiredAsync(cardNumber))
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Card is expired"]
            };        }

        return new ApiResponse
        {
            StatusCode = HttpStatusCode.OK,
            Result = "Card is valid"
        };
    }
    
    public async Task<ApiResponse> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            var existingCard = await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber);
            if (existingCard != null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Card already exists"]
                };
            }

            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankCardRegisterDto.Username);
            if (person == null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Person not found, username is incorrect"]
                };
            }

            var bankAccount =
                await unitOfWork.BankAccountRepository.GetAccountByIdAsync(bankCardRegisterDto.BankAccountId);
            if (bankAccount == null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Bank account not found"]
                };
            }

            var newCard = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = bankCardRegisterDto.Cvv,
                PinCode = bankCardRegisterDto.PinCode,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname,
                AccountId = bankAccount.Id
            };

            await unitOfWork.BankCardRepository.CreateCardAsync(newCard);
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true,
                Result = new 
                {
                    Firstname = newCard.Firstname,
                    Lastname = newCard.Lastname,
                    CardNumber = newCard.CardNumber,
                    ExpirationDate = newCard.ExpirationDate,
                    Cvv = newCard.Cvv,
                    PinCode = newCard.PinCode,
                    AccountId = newCard.AccountId
                }
            };

        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = ["Error creating card"]
            };
        }
    }
}
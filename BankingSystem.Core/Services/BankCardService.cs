using System.Net;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<ApiResponse> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankCardRegisterDto.Username);
            if (person is null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Person not found, username is incorrect"]
                };
            }

            var bankAccounts = await unitOfWork.BankAccountRepository.GetAccountsByIdAsync(bankCardRegisterDto.BankAccountId);

            if (bankAccounts.Count(b => b.Id == bankCardRegisterDto.BankAccountId) == 0)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var card = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = bankCardRegisterDto.Cvv,
                PinCode = bankCardRegisterDto.PinCode,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname,
                AccountId = bankCardRegisterDto.BankAccountId
            };

            await unitOfWork.BankCardRepository.CreateCardAsync(card);
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Created
            };

        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest
            };
        }
    }
}
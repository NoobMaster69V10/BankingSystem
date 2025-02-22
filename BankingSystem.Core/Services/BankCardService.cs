using System.Net;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<Result> ValidateCardAsync(string cardNumber, string pinCode)
    {
        if (!await unitOfWork.BankCardRepository.DoesCardExistAsync(cardNumber))
        {
            return Result.Failure(Error.NotFound("Card does not exist."));
        }
        if (!await unitOfWork.BankCardRepository.CheckPinCodeAsync(cardNumber, pinCode))
        {
            return Result.Failure(Error.AccessUnAuthorized("Pin code does not match."));
        }

        if (await unitOfWork.BankCardRepository.IsCardExpiredAsync(cardNumber))
        {
            return Result.Failure(Error.BadRequest("Card is expired"));
        }

        return Result.Success();
    }
    
    public async Task<ResultT<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            var existingCard = await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber);
            if (existingCard != null)
            {
                return ResultT<BankCard>.Failure(Error.BadRequest("Card already exists"));
            }

            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankCardRegisterDto.Username);
            if (person == null)
            {
                return ResultT<BankCard>.Failure(Error.NotFound("Person not found, username is incorrect"));
            }

            var bankAccount =
                await unitOfWork.BankAccountRepository.GetAccountByIdAsync(bankCardRegisterDto.BankAccountId);
            if (bankAccount == null)
            {
                return ResultT<BankCard>.Failure(Error.NotFound("Bank account not found"));
            }

            var newCard = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = bankCardRegisterDto.Cvv,
                PinCode = bankCardRegisterDto.PinCode,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname,
                AccountId = bankAccount.BankAccountId
            };

            await unitOfWork.BankCardRepository.CreateCardAsync(newCard);
            return ResultT<BankCard>.Success(newCard);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return ResultT<BankCard>.Failure(Error.ServerError("Error creating card"));
        }
    }
}

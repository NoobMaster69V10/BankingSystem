﻿using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<CustomResult<bool>> ValidateCardAsync(string cardNumber, string pinCode)
    {
        var cardExists = await unitOfWork.BankCardRepository.DoesCardExistAsync(cardNumber);
        if (!cardExists)
        {
            return CustomResult<bool>.Failure(CustomError.RecordNotFound("Card number not found"));
        }

        if (!await unitOfWork.BankCardRepository.CheckPinCodeAsync(cardNumber, pinCode))
        {
            return CustomResult<bool>.Failure(new CustomError("INVALID_PIN", "Pin code does not match"));
        }

        if (await unitOfWork.BankCardRepository.IsCardExpiredAsync(cardNumber))
        {
            return CustomResult<bool>.Failure(new CustomError("CARD_EXPIRED", "Card is expired"));
        }

        return CustomResult<bool>.Success(true);
    }

    public async Task<CustomResult<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            var existingCard = await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber);
            if (existingCard != null)
            {
                return CustomResult<BankCard>.Failure(new CustomError("CardAlreadyExists", "Card already exists"));
            }

            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankCardRegisterDto.Username);
            if (person == null)
            {
                return CustomResult<BankCard>.Failure(CustomError.RecordNotFound("Person Not Found"));
            }

            var bankAccount =
                await unitOfWork.BankAccountRepository.GetAccountByIdAsync(bankCardRegisterDto.BankAccountId);
            if (bankAccount == null)
            {
                return CustomResult<BankCard>.Failure(CustomError.RecordNotFound("Bank account not found"));
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
            return CustomResult<BankCard>.Success(newCard);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return CustomResult<BankCard>.Failure(new CustomError("asdasd", "asdasd"));
        }
    }
}
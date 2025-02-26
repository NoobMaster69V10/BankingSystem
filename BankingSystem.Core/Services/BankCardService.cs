using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Helpers;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<CustomResult<bool>> ValidateCardAsync(string cardNumber, string pinCode)
    {
        try
        {
            var cardExists = await unitOfWork.BankCardRepository.DoesCardExistAsync(cardNumber);
            if (!cardExists)
            {
                return CustomResult<bool>.Failure(CustomError.NotFound("Card number not found"));
            }

            if (!await unitOfWork.BankCardRepository.CheckPinCodeAsync(cardNumber, pinCode))
            {
                return CustomResult<bool>.Failure(CustomError.NotFound("Pin code does not match"));
            }

            if (await unitOfWork.BankCardRepository.IsCardExpiredAsync(cardNumber))
            {
                return CustomResult<bool>.Failure(new CustomError("CARD_EXPIRED", "Card is expired"));
            }

            return CustomResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return CustomResult<bool>.Failure(CustomError.Failure("Error occurred while validating card"));
        }
    }

    public async Task<CustomResult<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            var existingCard = await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber);
            if (existingCard != null)
            {
                return CustomResult<BankCard>.Failure(new CustomError("CardAlreadyExists", "Card already exists"));
            }

            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankCardRegisterDto.Username);
            if (person == null)
            {
                return CustomResult<BankCard>.Failure(CustomError.NotFound("Person Not Found"));
            }

            var bankAccount =
                await unitOfWork.BankAccountRepository.GetAccountByIdAsync(bankCardRegisterDto.BankAccountId);
            if (bankAccount == null)
            {
                return CustomResult<BankCard>.Failure(CustomError.NotFound("Bank account not found."));
            }

            if (!person.BankAccounts.Any())
            {
                return CustomResult<BankCard>.Failure(CustomError.NotFound($"'{bankCardRegisterDto.Username}' does not have accounts, please create accounts first!"));
            }


            if (person.BankAccounts.All(ba => ba.BankAccountId != bankCardRegisterDto.BankAccountId))
            {
                return CustomResult<BankCard>.Failure(CustomError.NotFound($"'{bankCardRegisterDto.Username}' does not have account with accountId - '{bankCardRegisterDto.BankAccountId}'!"));
            }

            var (hashedPin, hashedCvv, salt) = HashingHelper.HashPinAndCvv(bankCardRegisterDto.PinCode, bankCardRegisterDto.Cvv);

            var newCard = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = hashedCvv,
                PinCode = hashedPin,
                Salt = salt,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname,
                AccountId = bankAccount.BankAccountId
            };

            await unitOfWork.BankCardRepository.CreateCardAsync(newCard);
            await unitOfWork.CommitAsync();
            return CustomResult<BankCard>.Success(newCard);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole(ex.ToString());
            return CustomResult<BankCard>.Failure(CustomError.Failure("Bank card could not be created."));
        }
    }
}
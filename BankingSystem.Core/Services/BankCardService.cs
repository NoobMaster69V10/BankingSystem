using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Helpers;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<Result<bool>> ValidateCardAsync(string cardNumber, string pinCode)
    {
        try
        {
            var card = await unitOfWork.BankCardRepository.GetCardDetailsAsync(cardNumber);

            if (card == null)
            {
                return Result<bool>.Failure(CustomError.NotFound("Card number not found"));
            }

            if (!HashingHelper.VerifyHash(pinCode, card.Value.PinCode, card.Value.Salt))
            {
                return Result<bool>.Failure(CustomError.NotFound("Pin code does not match"));
            }

            if (card.Value.ExpiryDate < DateTime.UtcNow)
            {
                return Result<bool>.Failure(new CustomError("CARD_EXPIRED", "Card is expired"));
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return Result<bool>.Failure(CustomError.Failure("Error occurred while validating card"));
        }
    }

    
    public async Task<Result<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            var existingCard = await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber);
            if (existingCard != null)
            {
                return Result<BankCard>.Failure(new CustomError("CardAlreadyExists", "Card already exists"));
            }

            var person = await unitOfWork.PersonRepository.GetByUsernameAsync(bankCardRegisterDto.Username);
            if (person == null)
            {
                return Result<BankCard>.Failure(CustomError.NotFound("Person Not Found"));
            }

            var bankAccount = await unitOfWork.BankAccountRepository.GetByIdAsync(bankCardRegisterDto.BankAccountId);
            if (bankAccount == null)
            {
                return Result<BankCard>.Failure(CustomError.NotFound("Bank account not found."));
            }

            if (!person.BankAccounts.Any())
            {
                return Result<BankCard>.Failure(CustomError.NotFound(
                    $"'{bankCardRegisterDto.Username}' does not have accounts, please create accounts first!"));
            }


            if (person.BankAccounts.All(ba => ba.BankAccountId != bankCardRegisterDto.BankAccountId))
            {
                return Result<BankCard>.Failure(CustomError.NotFound(
                    $"'{bankCardRegisterDto.Username}' does not have account with accountId - '{bankCardRegisterDto.BankAccountId}'!"));
            }

            var (hashedPin, salt) =
                HashingHelper.HashPinAndCvv(bankCardRegisterDto.PinCode);


            var encryptedCvv = EncryptionHelper.Encrypt(bankCardRegisterDto.Cvv);

            var newCard = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = encryptedCvv,
                PinCode = hashedPin,
                Salt = salt,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname,
                AccountId = bankAccount.BankAccountId
            };

            await unitOfWork.BankCardRepository.AddAsync(newCard);
            await unitOfWork.CommitAsync();
            return Result<BankCard>.Success(newCard);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return Result<BankCard>.Failure(CustomError.Failure("Bank card could not be created."));
        }
    }
}
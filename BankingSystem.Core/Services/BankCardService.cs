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
                return Result<bool>.Failure(CustomError.Validation("Pin code does not match"));
            }

            return card.Value.ExpiryDate < DateTime.UtcNow ? Result<bool>.Failure(CustomError.Validation("Card is expired")) : Result<bool>.Success(true);
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
        if (await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber) is not null)
        {
            return Result<BankCard>.Failure(CustomError.Validation("A card with this number already exists. Please use a different card number."));
        }

        var person = await unitOfWork.PersonRepository.GetByUsernameAsync(bankCardRegisterDto.Username);
        if (person is null)
        {
            return Result<BankCard>.Failure(CustomError.NotFound("User not found. Please check the username and try again."));
        }

        var bankAccount = await unitOfWork.BankAccountRepository.GetByIdAsync(bankCardRegisterDto.BankAccountId);
        if (bankAccount is null)
        {
            return Result<BankCard>.Failure(CustomError.NotFound("The provided bank account does not exist. Please verify your details."));
        }

        if (!person.BankAccounts.Any())
        {
            return Result<BankCard>.Failure(CustomError.NotFound("You do not have any linked bank accounts. Please create an account first."));
        }

        if (person.BankAccounts.All(ba => ba.BankAccountId != bankAccount.BankAccountId))
        {
            return Result<BankCard>.Failure(CustomError.NotFound("This bank account is not associated with your profile. Please use a valid account."));
        }

        var (hashedPin, salt) = HashingHelper.HashPinAndCvv(bankCardRegisterDto.PinCode);
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
        loggerService.LogErrorInConsole($"[CreateBankCardAsync] An error occurred: {ex.Message}");

        return Result<BankCard>.Failure(CustomError.Failure("An unexpected error occurred while processing your request. Please try again later."));
    }
}

}
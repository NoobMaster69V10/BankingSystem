using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService, IHasherService hasherService, IEncryptionService encryptionService)
    : IBankCardService
{
    public async Task<Result<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            if (await unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber) is not null)
            {
                return Result<BankCard>.Failure(
                    CustomError.Validation(
                        "A card with this number already exists. Please use a different card number."));
            }

            var person = await unitOfWork.PersonRepository.GetByUsernameAsync(bankCardRegisterDto.Username);
            if (person is null)
            {
                return Result<BankCard>.Failure(
                    CustomError.NotFound("User not found. Please check the username and try again."));
            }

            var bankAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(bankCardRegisterDto.BankAccountId);
            if (bankAccount is null)
            {
                return Result<BankCard>.Failure(
                    CustomError.NotFound("The provided bank account does not exist. Please verify your details."));
            }

            if (!person.BankAccounts.Any())
            {
                return Result<BankCard>.Failure(
                    CustomError.NotFound("You do not have any linked bank accounts. Please create an account first."));
            }

            if (person.BankAccounts.All(ba => ba.BankAccountId != bankAccount.BankAccountId))
            {
                return Result<BankCard>.Failure(CustomError.NotFound(
                    "This bank account is not associated with your profile. Please use a valid account."));
            }

            var pinHash = hasherService.Hash(bankCardRegisterDto.PinCode);

            var encryptedCvv = encryptionService.Encrypt(bankCardRegisterDto.Cvv);

            var newCard = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = encryptedCvv,
                PinCode = pinHash,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                AccountId = bankAccount.BankAccountId
            };

            await unitOfWork.BankCardRepository.AddCardAsync(newCard);
            await unitOfWork.CommitAsync();

            return Result<BankCard>.Success(newCard);
        }
        catch (Exception ex)
        {
            loggerService.LogError($"[CreateBankCardAsync] An error occurred: {ex.Message}");

            return Result<BankCard>.Failure(
                CustomError.Failure(
                    "An unexpected error occurred while processing your request. Please try again later."));
        }
    }

    public async Task<Result<bool>> ValidateCardAsync(string cardNumber, string pinCode)
    {
        try
        {
            var card = await unitOfWork.BankCardRepository.GetCardDetailsAsync(cardNumber);

            if (card == null)
            {
                return Result<bool>.Failure(CustomError.NotFound("Card number not found"));
            }

            if (!hasherService.Verify(pinCode, card.Value.PinCode))
            {
                return Result<bool>.Failure(CustomError.Validation("Pin code does not match"));
            }

            return card.Value.ExpiryDate < DateTime.UtcNow
                ? Result<bool>.Failure(CustomError.Validation("Card is expired"))
                : Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            loggerService.LogError(ex.Message);
            return Result<bool>.Failure(CustomError.Failure("Error occurred while validating card"));
        }
    }
}
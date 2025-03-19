using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService : IBankCardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly IHasherService _hasherService;
    private readonly ILoggerService _loggerService;

    public BankCardService(IUnitOfWork unitOfWork, IHasherService hasherService, IEncryptionService encryptionService,
        ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _hasherService = hasherService;
        _encryptionService = encryptionService;
        _loggerService = loggerService;
    }

    public async Task<Result<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            if (await _unitOfWork.BankCardRepository.GetCardAsync(bankCardRegisterDto.CardNumber!, cancellationToken) is not null)
            {
                return Result<BankCard>.Failure(
                    CustomError.Validation(
                        "A card with this number already exists. Please use a different card number."));
            }

            var person = await _unitOfWork.PersonRepository.GetByUsernameAsync(bankCardRegisterDto.Username!, cancellationToken);
            if (person is null)
            {
                return Result<BankCard>.Failure(
                    CustomError.NotFound("User not found. Please check the username and try again."));
            }

            var bankAccount = await _unitOfWork.BankAccountRepository.GetAccountByIdAsync(bankCardRegisterDto.BankAccountId, cancellationToken);
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
                return Result<BankCard>.Failure(CustomError.NotFound("This bank account is not associated with your profile. Please use a valid account."));
            }

            var pinHash = _hasherService.Hash(bankCardRegisterDto.PinCode);

            var encryptedCvv = _encryptionService.Encrypt(bankCardRegisterDto.Cvv!);

            var newCard = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber!,
                Cvv = encryptedCvv,
                PinCode = pinHash,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                AccountId = bankAccount.BankAccountId
            };

            await _unitOfWork.BankCardRepository.AddCardAsync(newCard, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<BankCard>.Success(newCard);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"[CreateBankCardAsync] An error occurred: {ex.Message}");

            return Result<BankCard>.Failure(
                CustomError.Failure(
                    "An unexpected error occurred while processing your request. Please try again later."));
        }
    }
    
    public async Task<Result<CardRemovalResponse>> RemoveBankCardAsync(BankCardActiveDto bankCardActiveDto, CancellationToken cancellationToken = default)
    {
        var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(bankCardActiveDto.CardNumber!, cancellationToken);
        if (bankAccount == null || bankCardActiveDto.PersonId != bankAccount.PersonId)
        {
            return Result<CardRemovalResponse>.Failure(
                CustomError.AccessForbidden("Card not found"));
        }
        try
        {
            await _unitOfWork.BankCardRepository.RemoveBankCardAsync(bankCardActiveDto.CardNumber!, cancellationToken);
            var cardRemovalResponse = new CardRemovalResponse
            {
                CardNumber = bankCardActiveDto.CardNumber!,
                Message = "Card removed successfully"
            };
            return Result<CardRemovalResponse>.Success(cardRemovalResponse);
        }
        catch (Exception e)
        {
            _loggerService.LogError($"[RemoveBankCardAsync] An error occurred: {e.Message}");
            return Result<CardRemovalResponse>.Failure(
                CustomError.Failure("An error occurred while removing the card"));
        }
    }

    public async Task<Result<string>> DeactivateBankCardAsync(string cardNumber, string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(cardNumber, cancellationToken);
            if (bankAccount == null || userId != bankAccount.PersonId)
            {
                return Result<string>.Failure(
                    CustomError.NotFound("Card not found"));
            }
            if (!await _unitOfWork.BankCardRepository.GetCardStatusAsync(cardNumber, cancellationToken))
            {
                return Result<string>.Failure(
                    CustomError.Validation("Card is already deactivated"));
            }
            await _unitOfWork.BankCardRepository.DeactivateCardAsync(cardNumber, cancellationToken);
            return Result<string>.Success("Card deactivated successfully");
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex.Message);
            return Result<string>.Failure(CustomError.Failure("Error occurred while validating card"));
        }
    }

    public async Task<Result<string>> ActivateBankCardAsync(string cardNumber, string userId,
        CancellationToken cancellationToken = default)
    {
        var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(cardNumber, cancellationToken);
        if (bankAccount == null || userId != bankAccount.PersonId)
        {
            return Result<string>.Failure(
                CustomError.NotFound("Card not found"));
        }
        var card = await _unitOfWork.BankCardRepository.GetCardSecurityDetailsAsync(cardNumber, cancellationToken);
        if (card!.Value.ExpiryDate < DateTime.Now)
        {
            return Result<string>.Failure(CustomError.Validation("Card is expired, You can't activate it. Visit Bank"));
        }
        if (card.Value.IsActive)
        {
            return Result<string>.Success("Card is already active");
        }
        await _unitOfWork.BankCardRepository.ActivateBankCardAsync(cardNumber, cancellationToken);
        return Result<string>.Success("Card activated successfully");
    }

    public async Task<Result<bool>> ValidateCardAsync(string cardNumber, string pinCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var card = await _unitOfWork.BankCardRepository.GetCardSecurityDetailsAsync(cardNumber, cancellationToken);
            if (card == null)
            {
                return Result<bool>.Failure(CustomError.NotFound("Card not found"));
            }

            if (!card.Value.IsActive)
            {
                return Result<bool>.Failure(CustomError.Validation("Card is not active"));
            }

            if (!_hasherService.Verify(pinCode, card.Value.PinCode))
            {
                return Result<bool>.Failure(CustomError.Validation("Pin code does not match"));
            }

            return card.Value.ExpiryDate < DateTime.UtcNow
                ? Result<bool>.Failure(CustomError.Validation("Card is expired"))
                : Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex.Message);
            return Result<bool>.Failure(CustomError.Failure("Error occurred while validating card"));
        }
    }
}
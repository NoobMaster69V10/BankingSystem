using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Helpers;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AtmService : IAtmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankCardService _bankCardService;
    private readonly ILoggerService _loggerService;
    private readonly IHasherService _hasherService;

    public AtmService(IUnitOfWork unitOfWork, IBankCardService bankCardService, ILoggerService loggerService, IHasherService hasherService)
    {
        _unitOfWork = unitOfWork;
        _bankCardService = bankCardService;
        _loggerService = loggerService;
        _hasherService = hasherService;
    }


    public async Task<Result<BalanceResponseDto>> ShowBalanceAsync(CardAuthorizationDto cardDto)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode);
            if (authResult.IsFailure)
            {
                if (authResult.Error != null) return Result<BalanceResponseDto>.Failure(authResult.Error);
            }

            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber);
            var response = new BalanceResponseDto(
                balance: balance,
                cardNumber: cardDto.CardNumber
            );

            return Result<BalanceResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ShowBalanceAsync: {ex}");
            return Result<BalanceResponseDto>.Failure(
                new CustomError("BALANCE_ERROR", "An error occurred while retrieving the balance")
            );
        }
    }

    public async Task<Result<bool>> ChangePinAsync(ChangePinDto changePinDto)
    {
        try
        {
            if (changePinDto.CurrentPin == changePinDto.NewPin)
            {
                return Result<bool>.Failure(new CustomError("PIN_SAME_ERROR", "Current PIN and new PIN cannot be the same."));
            }
            var authResult = await AuthorizeCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin);
            if (!authResult.IsSuccess)
            {
                if (authResult.Error != null) return Result<bool>.Failure(authResult.Error);
            }
            
            var pinHash = _hasherService.Hash(changePinDto.NewPin);
         
            await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber,pinHash);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in ChangePinAsync: {ex}");
            return Result<bool>.Failure(
                new CustomError("PIN_CHANGE_ERROR", "An error occurred while changing the PIN")
            );
        }
    } 
    public async Task<Result<bool>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto)
    {
        try
        {
            if (withdrawMoneyDto.Amount <= 0)
            {
                return Result<bool>.Failure(new CustomError("AmountLessOrEqualZero", "Amount must be greater than 0."));
            }

            if (withdrawMoneyDto.Amount > 10000)
            {
                return Result<bool>.Failure(new CustomError("AmountGreaterOrEqualZero", "Amount must be less or equal to 10000."));
            }
            var validated = await _bankCardService.ValidateCardAsync(withdrawMoneyDto.CardNumber, withdrawMoneyDto.PinCode);
            if (!validated.IsSuccess)
            {
                return Result<bool>.Failure(validated.Error);
            }
            var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawMoneyDto.CardNumber);
            if (bankAccount == null)
            {
                return Result<bool>.Failure(CustomError.NotFound("Bank account not found."));
            }
            var totalWithdrawnToday = await _unitOfWork.AtmRepository.GetTotalWithdrawnTodayAsync(bankAccount.BankAccountId);
            if (totalWithdrawnToday + withdrawMoneyDto.Amount > 10000)
            {
                return Result<bool>.Failure(new CustomError("DailyLimitExceeded", "You cannot withdraw more than $10,000 per day."));
            }
            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(withdrawMoneyDto.CardNumber);

            var fee = withdrawMoneyDto.Amount * 0.02m;
            var totalDeduction = withdrawMoneyDto.Amount + fee;

            if (balance < totalDeduction)
            {
                return Result<bool>.Failure(new CustomError("NotEnoughBalance", "Not enough balance including the transaction fee."));
            }

            var newBalance = balance - totalDeduction;
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, newBalance);
            
            var atmTransaction = new AtmTransaction
            {
                Amount = withdrawMoneyDto.Amount,
                Currency = withdrawMoneyDto.Currency,
                TransactionDate = DateTime.UtcNow,
                AccountId = bankAccount.BankAccountId,
                TransactionFee = fee
            };

            await _unitOfWork.TransactionRepository.AddAtmTransactionAsync(atmTransaction);
            await _unitOfWork.CommitAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in WithdrawMoneyAsync: {ex}");
            return Result<bool>.Failure(CustomError.Failure("An error occurred during the transaction."));
        }
    }

    private async Task<Result<bool>> AuthorizeCardAsync(string cardNumber, string pin)
    {
        try
        {
            var validationResult = await _bankCardService.ValidateCardAsync(cardNumber, pin);
            if (!validationResult.IsSuccess)
            {
                if (validationResult.Error != null) return Result<bool>.Failure(validationResult.Error);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _loggerService.LogErrorInConsole($"Error in AuthorizeCardAsync: {ex}");
            return Result<bool>.Failure(new CustomError("AUTH_ERROR", "Error occurred while authorizing"));
        }
    }
}
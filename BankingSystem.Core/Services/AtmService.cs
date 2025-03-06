using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AtmService : IAtmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankCardService _bankCardService;
    private readonly ILoggerService _loggerService;
    private readonly IHasherService _hasherService;
    private readonly IExchangeRateApi _exchangeRateApi;

    public AtmService(IUnitOfWork unitOfWork, IBankCardService bankCardService, ILoggerService loggerService,
        IHasherService hasherService, IExchangeRateApi exchangeRateApi)
    {
        _unitOfWork = unitOfWork;
        _bankCardService = bankCardService;
        _loggerService = loggerService;
        _hasherService = hasherService;
        _exchangeRateApi = exchangeRateApi;
    }


    public async Task<Result<BalanceResponse>> ShowBalanceAsync(CardAuthorizationDto cardDto)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode);
            if (authResult.IsFailure)
            {
                if (authResult.Error != null) return Result<BalanceResponse>.Failure(authResult.Error);
            }

            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber);
            var response = new BalanceResponse
            {
                CardNumber = cardDto.CardNumber,
                Balance = balance,
            };
            return Result<BalanceResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Error in ShowBalanceAsync: {ex}");
            return Result<BalanceResponse>.Failure(
                new CustomError("BALANCE_ERROR", "An error occurred while retrieving the balance")
            );
        }
    }

    public async Task<Result<string>> ChangePinAsync(ChangePinDto changePinDto)
    {
        try
        {
            if (changePinDto.PinCode == changePinDto.NewPin)
            {
                return Result<string>.Failure(new CustomError("PIN_SAME_ERROR",
                    "Current PIN and new PIN cannot be the same."));
            }

            var authResult = await AuthorizeCardAsync(changePinDto.CardNumber, changePinDto.PinCode);
            if (!authResult.IsSuccess)
            {
                if (authResult.Error != null) return Result<string>.Failure(authResult.Error);
            }

            var pinHash = _hasherService.Hash(changePinDto.NewPin);

            await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, pinHash);
            return Result<string>.Success("Pin changed Successfully");
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Error in ChangePinAsync: {ex}");
            return Result<string>.Failure(
                new CustomError("PIN_CHANGE_ERROR", "An error occurred while changing the PIN")
            );
        }
    }

    public async Task<Result<AtmTransactionResponse>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto)
    {
        try
        {
            if (withdrawMoneyDto.Amount % 1 != 0)
            {
                return Result<AtmTransactionResponse>.Failure(new CustomError("InvalidAmount",
                    "Withdrawals must be in whole numbers (paper money only)."));
            }

            if (withdrawMoneyDto.Amount <= 0)
            {
                return Result<AtmTransactionResponse>.Failure(new CustomError("AmountLessOrEqualZero",
                    "Amount must be greater than 0."));
            }

            var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawMoneyDto.CardNumber);
            if (bankAccount == null)
            {
                return Result<AtmTransactionResponse>.Failure(CustomError.NotFound("Bank account not found."));
            }

            var totalWithdrawnTodayInGel = await GetTotalWithdrawnTodayInGelAsync(bankAccount);
            decimal withdrawAmountInGel = withdrawMoneyDto.Amount;
            if (bankAccount.Currency != Currency.GEL)
            {
                var exchangeRate = await _exchangeRateApi.GetExchangeRate(bankAccount.Currency);
                if (exchangeRate <= 0)
                {
                    return Result<AtmTransactionResponse>.Failure(new CustomError("ExchangeRateError",
                        "Failed to retrieve exchange rate."));
                }

                withdrawAmountInGel = withdrawMoneyDto.Amount * exchangeRate;
            }

            var dailyLimit = 10000;

            if (totalWithdrawnTodayInGel.Value + withdrawAmountInGel > dailyLimit)
            {
                return Result<AtmTransactionResponse>.Failure(new CustomError("DailyLimitExceeded",
                    "You cannot withdraw more than 10,000 GEL per day."));
            }

            var fee = withdrawMoneyDto.Amount * 0.02m;
            var totalDeduction = withdrawMoneyDto.Amount + fee;
            if (bankAccount.Balance < totalDeduction)
            {
                return Result<AtmTransactionResponse>.Failure(new CustomError("NotEnoughBalance",
                    "Not enough balance including the transaction fee."));
            }

            await _unitOfWork.BeginTransactionAsync();

            bankAccount.Balance -= totalDeduction;
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount);

            var atmTransaction = new AtmTransaction
            {
                FromAccountId = bankAccount.BankAccountId,
                Amount = withdrawMoneyDto.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionFee = fee
            };
            await _unitOfWork.BankTransactionRepository.AddAtmTransactionAsync(atmTransaction);
            await _unitOfWork.CommitAsync();

            var response = new AtmTransactionResponse
            {
                Amount = withdrawMoneyDto.Amount,
                Currency = bankAccount.Currency,
                Iban = bankAccount.Iban!
            };
            return Result<AtmTransactionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Error in WithdrawMoneyAsync: {ex}");
            return Result<AtmTransactionResponse>.Failure(
                CustomError.Failure("An error occurred during the transaction."));
        }
    }

    public async Task<Result<BalanceResponse>> DepositMoneyAsync(DepositMoneyDto cardDto)
    {
        try
        {
            if (cardDto.Amount <= 0)
            {
                return Result<BalanceResponse>.Failure(new CustomError("AmountLessOrEqualZero",
                    "Amount must be greater than 0."));
            }

            var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(cardDto.CardNumber);
            if (bankAccount == null)
            {
                return Result<BalanceResponse>.Failure(CustomError.NotFound("Bank account not found."));
            }
            bankAccount.Balance += cardDto.Amount;
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount);
            var response = new BalanceResponse
            {
                CardNumber = cardDto.CardNumber,
                Balance = bankAccount.Balance
            };

            return Result<BalanceResponse>.Success(response);
        }
        catch (Exception e)
        {
            _loggerService.LogError($"Error in WithdrawMoneyAsync:{e}");
            return Result<BalanceResponse>.Failure(CustomError.NotFound("Error during transaction."));
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
            _loggerService.LogError($"Error in AuthorizeCardAsync: {ex}");
            return Result<bool>.Failure(new CustomError("AUTH_ERROR", "Error occurred while authorizing"));
        }
    }

    private async Task<Result<decimal>> GetTotalWithdrawnTodayInGelAsync(BankAccount bankAccount)
    {
        try
        {
            var totalWithdrawnToday =
                await _unitOfWork.BankTransactionRepository.GetTotalWithdrawnTodayAsync(bankAccount.BankAccountId);

            if (bankAccount.Currency != Currency.GEL)
            {
                var exchangeRate = await _exchangeRateApi.GetExchangeRate(bankAccount.Currency);
                if (exchangeRate <= 0)
                {
                    return Result<decimal>.Failure(
                        new CustomError("ExchangeError", "Failed to retrieve exchange rate."));
                }

                totalWithdrawnToday *= exchangeRate;
            }

            return Result<decimal>.Success(totalWithdrawnToday);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Error in ShowBalanceAsync: {ex}");
            return Result<decimal>.Failure(new CustomError("Error", "Error during getting money"));
        }
    }
}
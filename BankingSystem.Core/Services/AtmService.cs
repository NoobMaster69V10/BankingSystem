using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.ConfigurationSettings.AtmTransaction;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace BankingSystem.Core.Services;

public class AtmService : IAtmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankCardService _bankCardService;
    private readonly IHasherService _hasherService;
    private readonly ICurrencyExchangeClient _currencyExchangeClient;
    private readonly AtmTransactionSettings _atmTransactionSettings;
    private readonly ILoggerService _loggerService;

    public AtmService(IUnitOfWork unitOfWork, IBankCardService bankCardService, IOptions<AtmTransactionSettings> atmTransactionSettings,
        IHasherService hasherService, ICurrencyExchangeClient currencyExchangeClient, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _bankCardService = bankCardService;
        _hasherService = hasherService;
        _currencyExchangeClient = currencyExchangeClient;
        _atmTransactionSettings = atmTransactionSettings.Value;
        _loggerService = loggerService;
    }


    public async Task<Result<BalanceResponse>> ShowBalanceAsync(CardAuthorizationDto cardDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode, cancellationToken);
            if (authResult.IsFailure)
            {
                if (authResult.Error != null) return Result<BalanceResponse>.Failure(authResult.Error);
            }

            var balance = await _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber, cancellationToken);
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

    public async Task<Result<string>> ChangePinAsync(ChangePinDto changePinDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(changePinDto.CardNumber, changePinDto.PinCode, cancellationToken);
            if (!authResult.IsSuccess)
            {
                if (authResult.Error != null) return Result<string>.Failure(authResult.Error);
            }

            var pinHash = _hasherService.Hash(changePinDto.NewPin);

            await _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, pinHash, cancellationToken);
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

    public async Task<Result<AtmTransactionResponse>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(withdrawMoneyDto.CardNumber, withdrawMoneyDto.PinCode, cancellationToken);
            if (!authResult.IsSuccess)
            {
                if (authResult.Error != null) return Result<AtmTransactionResponse>.Failure(authResult.Error);
            }
            var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawMoneyDto.CardNumber, cancellationToken);
            if (bankAccount == null)
            {
                return Result<AtmTransactionResponse>.Failure(CustomError.NotFound("Bank account not found."));
            }

            var totalWithdrawnTodayInGel = await GetTotalWithdrawnTodayInGelAsync(bankAccount, cancellationToken);
            decimal withdrawAmountInGel = withdrawMoneyDto.Amount;
            if (bankAccount.Currency != Currency.GEL)
            {
                var exchangeRate = await _currencyExchangeClient.GetExchangeRateAsync(bankAccount.Currency, cancellationToken);
                if (exchangeRate <= 0)
                {
                    return Result<AtmTransactionResponse>.Failure(new CustomError("ExchangeRateError",
                        "Failed to retrieve exchange rate."));
                }

                withdrawAmountInGel = withdrawMoneyDto.Amount * exchangeRate;
            }

            var dailyLimit = _atmTransactionSettings.DailyLimit;

            if (totalWithdrawnTodayInGel.Value + withdrawAmountInGel > dailyLimit)
            {
                return Result<AtmTransactionResponse>.Failure(new CustomError("DailyLimitExceeded",
                    $"You cannot withdraw more than {dailyLimit} GEL per day."));
            }

            var fee = withdrawMoneyDto.Amount * _atmTransactionSettings.WithdrawalFee;
            var totalDeduction = withdrawMoneyDto.Amount + fee;
            if (bankAccount.Balance < totalDeduction)
            {
                return Result<AtmTransactionResponse>.Failure(new CustomError("NotEnoughBalance",
                    "Not enough balance including the transaction fee."));
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            bankAccount.Balance -= totalDeduction;
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, cancellationToken);

            var atmTransaction = new AtmTransaction
            {
                FromAccountId = bankAccount.BankAccountId,
                Amount = withdrawMoneyDto.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionFee = fee
            };
            await _unitOfWork.BankTransactionRepository.AddAtmTransactionAsync(atmTransaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

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
            await _unitOfWork.RollbackAsync();
            return Result<AtmTransactionResponse>.Failure(
                CustomError.Failure("An error occurred during the transaction."));
        }
    }

    public async Task<Result<BalanceResponse>> DepositMoneyAsync(DepositMoneyDto cardDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var authResult = await AuthorizeCardAsync(cardDto.CardNumber, cardDto.PinCode, cancellationToken);
            if (!authResult.IsSuccess)
            {
                if (authResult.Error != null) return Result<BalanceResponse>.Failure(authResult.Error);
            }
            var bankAccount = await _unitOfWork.BankCardRepository.GetAccountByCardAsync(cardDto.CardNumber, cancellationToken);
            if (bankAccount == null)
            {
                return Result<BalanceResponse>.Failure(CustomError.NotFound("Bank account not found."));
            }
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            bankAccount.Balance += cardDto.Amount;
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, cancellationToken);
            var atmTransaction = new AtmTransaction
            {
                FromAccountId = bankAccount.BankAccountId,
                Amount = cardDto.Amount,
                TransactionDate = DateTime.UtcNow,
            };
            await _unitOfWork.BankTransactionRepository.AddAtmTransactionAsync(atmTransaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
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
            await _unitOfWork.RollbackAsync();
            return Result<BalanceResponse>.Failure(CustomError.NotFound("Error during transaction."));
        }
    }

    private async Task<Result<bool>> AuthorizeCardAsync(string cardNumber, string pin, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _bankCardService.ValidateCardAsync(cardNumber, pin, cancellationToken);
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

    private async Task<Result<decimal>> GetTotalWithdrawnTodayInGelAsync(BankAccount bankAccount, CancellationToken cancellationToken = default)
    {
        try
        {
            var totalWithdrawnToday =
                await _unitOfWork.BankTransactionRepository.GetTotalWithdrawnTodayAsync(bankAccount.BankAccountId, cancellationToken);

            if (bankAccount.Currency != Currency.GEL)
            {
                var exchangeRate = await _currencyExchangeClient.GetExchangeRateAsync(bankAccount.Currency, cancellationToken);
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
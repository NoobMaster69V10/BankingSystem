﻿using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.AtmTransaction;

namespace BankingSystem.Core.Services;

public class AccountTransactionService(
    IUnitOfWork unitOfWork,
    IExchangeRateApi exchangeRateApi,
    ILoggerService loggerService,IBankCardService bankCardService) : IAccountTransactionService
{
   public async Task<Result<AccountTransaction>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId)
    {
        try
        {
            if (transactionDto.FromAccountId == transactionDto.ToAccountId)
            {
                return Result<AccountTransaction>.Failure(CustomError.Validation("It is not possible to make a transaction between the same accounts."));
            }

            await unitOfWork.BeginTransactionAsync();

            var fromAccount = await unitOfWork.BankAccountRepository.GetByIdAsync(transactionDto.FromAccountId);
            var toAccount = await unitOfWork.BankAccountRepository.GetByIdAsync(transactionDto.ToAccountId);

            if (fromAccount is null)
            {
                return Result<AccountTransaction>.Failure(CustomError.Validation($"Bank account with id '{transactionDto.FromAccountId}' not found!"));
            }

            if (toAccount is null)
            {
                return Result<AccountTransaction>.Failure(CustomError.Validation($"Bank account with id '{transactionDto.ToAccountId}' not found!"));
            }

            if (fromAccount.PersonId != userId)
            {
                return Result<AccountTransaction>.Failure(CustomError.Validation("You don't have permission to make transactions from this account."));
            }

            decimal transactionFee = 0;
            if (fromAccount.PersonId != toAccount.PersonId)
            {
                transactionFee = transactionDto.Amount * 0.01m + 0.5m;
            }

            if (transactionDto.Amount + transactionFee > fromAccount.Balance)
            {
                return Result<AccountTransaction>.Failure(CustomError.Validation("Insufficient balance for this transaction."));
            }

            var transaction = new AccountTransaction
            {
                FromAccountId = transactionDto.FromAccountId,
                ToAccountId = transactionDto.ToAccountId,
                Currency = fromAccount.Currency,
                Amount = transactionDto.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionFee = transactionFee
            };

            fromAccount.Balance -= (transactionDto.Amount + transactionFee);

            toAccount.Balance += await ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency);

            await unitOfWork.BankAccountRepository.UpdateAsync(fromAccount);
            await unitOfWork.BankAccountRepository.UpdateAsync(toAccount);
            await unitOfWork.TransactionRepository.AddAsync(transaction);
            await unitOfWork.CommitAsync();

            return Result<AccountTransaction>.Success(transaction);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole(ex.Message);

            return Result<AccountTransaction>.Failure(CustomError.Failure("An error occurred during the transaction."));
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
            var validated = await bankCardService.ValidateCardAsync(withdrawMoneyDto.CardNumber, withdrawMoneyDto.PinCode);
            if (!validated.IsSuccess)
            {
                return Result<bool>.Failure(validated.Error);
            }
            var bankAccount = await unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawMoneyDto.CardNumber);
            if (bankAccount == null)
            {
                return Result<bool>.Failure(CustomError.NotFound("Bank account not found."));
            }
            var totalWithdrawnToday = await unitOfWork.TransactionRepository.GetTotalWithdrawnTodayAsync(bankAccount.BankAccountId);
            if (totalWithdrawnToday + withdrawMoneyDto.Amount > 10000)
            {
                return Result<bool>.Failure(new CustomError("DailyLimitExceeded", "You cannot withdraw more than $10,000 per day."));
            }
            var balance = await unitOfWork.BankCardRepository.GetBalanceAsync(withdrawMoneyDto.CardNumber);

            decimal fee = withdrawMoneyDto.Amount * 0.02m;
            decimal totalDeduction = withdrawMoneyDto.Amount + fee;

            if (balance < totalDeduction)
            {
                return Result<bool>.Failure(new CustomError("NotEnoughBalance", "Not enough balance including the transaction fee."));
            }

            var newBalance = balance - totalDeduction;
            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, newBalance);


            var atmTransaction = new AtmTransaction
            {
                Amount = withdrawMoneyDto.Amount,
                Currency = withdrawMoneyDto.Currency,
                TransactionDate = DateTime.UtcNow,
                AccountId = bankAccount.BankAccountId
            };

            await unitOfWork.TransactionRepository.AddAtmTransactionAsync(atmTransaction);
            await unitOfWork.CommitAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole($"Error in WithdrawMoneyAsync: {ex}");
            return Result<bool>.Failure(CustomError.Failure("An error occurred during the transaction."));
        }
    }

    private async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rates = new Dictionary<string, decimal>
        {
            { "USD", await exchangeRateApi.GetExchangeRate("USD") },
            { "EUR", await exchangeRateApi.GetExchangeRate("EUR") }
        };

        return fromCurrency switch
        {
            "GEL" when rates.ContainsKey(toCurrency) => amount / rates[toCurrency],
            "USD" when toCurrency == "GEL" => amount * rates["USD"],
            "EUR" when toCurrency == "GEL" => amount * rates["EUR"],
            "USD" when toCurrency == "EUR" => amount * (rates["EUR"] / rates["USD"]),
            "EUR" when toCurrency == "USD" => amount * (rates["USD"] / rates["EUR"]),
            _ => amount
        };
    }
}
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Core.Services;

public class AccountTransactionService(
    IUnitOfWork unitOfWork,
    IExchangeRateApi exchangeRateApi,
    ILoggerService loggerService) : IAccountTransactionService
{
   public async Task<Result<AccountTransfer>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId)
    {
        try
        {
            if (transactionDto.FromAccountId == transactionDto.ToAccountId)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation("It is not possible to make a transaction between the same accounts."));
            }

            await unitOfWork.BeginTransactionAsync();

            var fromAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId);
            var toAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId);

            if (fromAccount is null)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation($"Bank account with  '{transactionDto.FromAccountId}' not found!"));
            }

            if (toAccount is null)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation($"Bank account with id '{transactionDto.ToAccountId}' not found!"));
            }

            if (fromAccount.PersonId != userId)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation("You don't have permission to make transactions from this account."));
            }

            decimal transactionFee = 0;
            if (fromAccount.PersonId != toAccount.PersonId)
            {
                transactionFee = transactionDto.Amount * 0.01m + 0.5m;
            }

            if (transactionDto.Amount + transactionFee > fromAccount.Balance)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation("Insufficient balance for this transaction."));
            }

            var transaction = new AccountTransfer
            {
                FromAccountId = transactionDto.FromAccountId,
                ToAccountId = transactionDto.ToAccountId,
                Amount = transactionDto.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionFee = transactionFee
            };

            fromAccount.Balance -= (transactionDto.Amount + transactionFee);

            toAccount.Balance += await ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency);

            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(fromAccount);
            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(toAccount);
            await unitOfWork.BankTransactionRepository.AddAccountTransferAsync(transaction);
            await unitOfWork.CommitAsync();

            return Result<AccountTransfer>.Success(transaction);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogError(ex.Message);

            return Result<AccountTransfer>.Failure(CustomError.Failure("An error occurred during the transaction."));
        }
    }
   
    private async Task<decimal> ConvertCurrencyAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rates = new Dictionary<Currency, decimal>
        {
            { Currency.USD, await exchangeRateApi.GetExchangeRate(Currency.USD) },
            { Currency.EUR, await exchangeRateApi.GetExchangeRate(Currency.EUR) }
        };

        return fromCurrency switch
        {
            Currency.GEL when rates.ContainsKey(toCurrency) => amount / rates[toCurrency],
            Currency.USD when toCurrency == Currency.GEL => amount * rates[Currency.USD],
            Currency.EUR when toCurrency == Currency.GEL => amount * rates[Currency.EUR],
            Currency.USD when toCurrency == Currency.EUR => amount * (rates[Currency.EUR] / rates[Currency.USD]),
            Currency.EUR when toCurrency == Currency.USD => amount * (rates[Currency.USD] / rates[Currency.EUR]),
            _ => amount
        };
    }
}
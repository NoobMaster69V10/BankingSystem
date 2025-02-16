using System.Net;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AccountTransactionService(
    IUnitOfWork unitOfWork,
    IExchangeRateApi exchangeRateApi,
    ILoggerService loggerService) : IAccountTransactionService
{
    public async Task<ApiResponse> TransactionBetweenAccountsAsync(TransactionDto transactionDto, string userId)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var fromAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId);
            var toAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId);

            if (fromAccount.PersonId != userId)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    IsSuccess = false,
                    ErrorMessages = ["You don't have permission to make transactions from this account."]
                };
            }

            decimal transactionFee = 0;
            if (fromAccount.PersonId != toAccount.PersonId)
            {
                transactionFee = transactionDto.Amount * 0.01m + 0.5m;
            }

            if (transactionDto.Amount + transactionFee > fromAccount.Balance)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Insufficient balance for this transaction."]
                };
            }

            var transaction = new AccountTransaction
            {
                FromAccountId = transactionDto.FromAccountId,
                ToAccountId = transactionDto.ToAccountId,
                Currency = fromAccount.Currency,
                Amount = transactionDto.Amount,
                TransactionDate = DateTime.UtcNow,
            };

            fromAccount.Balance -= transactionDto.Amount + transactionFee;
            transaction.Amount =
                await ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency);
            toAccount.Balance += transaction.Amount;

            await unitOfWork.BankAccountRepository.UpdateAccountAsync(fromAccount);
            await unitOfWork.BankAccountRepository.UpdateAccountAsync(toAccount);
            await unitOfWork.TransactionRepository.AddAccountTransactionAsync(transaction);
            await unitOfWork.CommitAsync();

            return new ApiResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new
                {
                    Message = "Transaction completed successfully.",
                    TransactionId = transaction.Id,
                    FromAccountBalance = fromAccount.Balance,
                    ToAccountBalance = toAccount.Balance
                }
            };
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole(ex.ToString());

            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = ["An error occurred during the transaction."]
            };
        }
    }


    public async Task<ApiResponse> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var bankAccount = await unitOfWork.BankCardRepository.GetAccountAsync(withdrawMoneyDto.CardNumber);
            if (bankAccount == null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["This bank account does not exist."]
                };
            }

            var balance = await unitOfWork.BankCardRepository.GetBalanceAsync(withdrawMoneyDto.CardNumber);
            if (balance < withdrawMoneyDto.Amount)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Insufficient funds."]
                };
            }

            var newBalance = balance - withdrawMoneyDto.Amount;
            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, newBalance);

            var atmTransaction = new AtmTransaction
            {
                Amount = withdrawMoneyDto.Amount,
                Currency = withdrawMoneyDto.Currency,
                TransactionDate = DateTime.UtcNow,
                AccountId = bankAccount.Id
            };

            await unitOfWork.TransactionRepository.AddAtmTransactionAsync(atmTransaction);
            await unitOfWork.CommitAsync();

            return new ApiResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new
                {
                    Message = "Withdrawal successful.",
                    NewBalance = newBalance,
                    withdrawMoneyDto.CardNumber
                }
            };
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole($"Error in WithdrawMoneyAsync: {ex}");

            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = ["An error occurred while processing the withdrawal."]
            };
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
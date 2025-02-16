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
    public async Task<string> TransactionBetweenAccountsAsync(TransactionDto transactionDto, string userId)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            var fromAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId);
            var toAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId);

            if (fromAccount.PersonId == userId)
            {
                var transaction = new AccountTransaction
                {
                    FromAccountId = transactionDto.FromAccountId,
                    ToAccountId = transactionDto.ToAccountId,
                    Currency = fromAccount.Currency,
                    Amount = transactionDto.Amount,
                    TransactionDate = DateTime.Now,
                    FromAtm = false
                };

                if (fromAccount.PersonId != toAccount.PersonId)
                {
                    var transactionFee = transaction.Amount * 0.01m + 0.5m;
                    if ((transaction.Amount + transactionFee) > fromAccount.Balance)
                    {
                        return "The transaction was failed. You don't have enough money";
                    }

                    fromAccount.Balance -= transaction.Amount + transactionFee;
                    transaction.Amount =
                        await ConvertCurrencyAsync(transaction.Amount, fromAccount.Currency, toAccount.Currency);
                    toAccount.Balance += transaction.Amount;
                    await unitOfWork.BankAccountRepository.UpdateAccountAsync(fromAccount);
                    await unitOfWork.BankAccountRepository.UpdateAccountAsync(toAccount);
                    await unitOfWork.TransactionRepository.AddAccountTransactionAsync(transaction);

                    await unitOfWork.CommitAsync();
                    return "The transaction was completed successfully.";
                }

                if (transaction.Amount > fromAccount.Balance)
                {
                    return "The transaction was failed. You don't have enough money";
                }

                fromAccount.Balance -= transaction.Amount;
                transaction.Amount =
                    await ConvertCurrencyAsync(transaction.Amount, fromAccount.Currency, toAccount.Currency);
                toAccount.Balance += transaction.Amount;
                await unitOfWork.BankAccountRepository.UpdateAccountAsync(fromAccount);
                await unitOfWork.BankAccountRepository.UpdateAccountAsync(toAccount);
                await unitOfWork.TransactionRepository.AddAccountTransactionAsync(transaction);

                await unitOfWork.CommitAsync();
                return "The transaction was completed successfully.";
            }

            return "Your don't have account with this id";
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole(ex.ToString());
            return "The transaction was failed.";
        }
    }

    public async Task<ApiResponse> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto)
    {
        try
        {
            var bankAccount = await unitOfWork.BankCardRepository.GetAccountAsync(withdrawMoneyDto.CardNumber);
            if (bankAccount == null)
            {
                return new ApiResponse()
                {
                    IsSuccess = false,
                    ErrorMessages = ["This Bank Account does not exist."]
                };
            }

            var balance = await unitOfWork.BankCardRepository.GetBalanceAsync(withdrawMoneyDto.CardNumber);
            if (balance < withdrawMoneyDto.Amount)
            {
                return new ApiResponse()
                {
                    IsSuccess = false,
                    ErrorMessages = ["You don't have enough money."]
                };
            }

            var newAmount = balance - withdrawMoneyDto.Amount;

            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, newAmount); 
            
            var atmTransaction = new AtmTransaction
            { 
                Amount = withdrawMoneyDto.Amount,
                Currency = withdrawMoneyDto.Currency,
                TransactionDate = DateTime.Now, 
                AccountId = bankAccount.Id
            };
            await unitOfWork.TransactionRepository.AddAtmTransactionAsync(atmTransaction);
            
            await unitOfWork.CommitAsync();

            return new ApiResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = new 
                {
                    NewBalance = newAmount, withdrawMoneyDto.CardNumber
                }
            };
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return new ApiResponse()
            {
                IsSuccess = false,
                ErrorMessages = ["An error occured."]
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
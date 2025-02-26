using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AccountTransactionService(
    IUnitOfWork unitOfWork,
    IExchangeRateApi exchangeRateApi,
    ILoggerService loggerService,IBankCardService bankCardService) : IAccountTransactionService
{
    public async Task<CustomResult<AccountTransaction>> TransactionBetweenAccountsAsync(TransactionDto transactionDto, string userId)
    {
        try
        {
            if (transactionDto.FromAccountId == transactionDto.ToAccountId)
            {
                return CustomResult<AccountTransaction>.Failure(CustomError.Validation("It is not possible to make a transaction between the same accounts."));
            }

            await unitOfWork.BeginTransactionAsync();

            var fromAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId);
            var toAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId);

            if (fromAccount is null)
            {
                return CustomResult<AccountTransaction>.Failure(CustomError.Validation($"Bank account with id '{transactionDto.FromAccountId}' not found!"));
            }

            if (toAccount is null)
            {
                return CustomResult<AccountTransaction>.Failure(CustomError.Validation($"Bank account with id '{transactionDto.ToAccountId}' not found!"));
            }

            if (fromAccount.PersonId != userId)
            {
                return CustomResult<AccountTransaction>.Failure(CustomError.Validation("You don't have permission to make transactions from this account."));
            }

            decimal transactionFee = 0;
            if (fromAccount.PersonId != toAccount!.PersonId)
            {
                transactionFee = transactionDto.Amount * 0.01m + 0.5m;
            }

            if (transactionDto.Amount + transactionFee > fromAccount.Balance)
            {
                return CustomResult<AccountTransaction>.Failure(CustomError.Validation("Insufficient balance for this transaction."));
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

            await unitOfWork.BankAccountRepository.UpdateAccountAsync(fromAccount);
            await unitOfWork.BankAccountRepository.UpdateAccountAsync(toAccount);
            await unitOfWork.TransactionRepository.AddAccountTransactionAsync(transaction);
            await unitOfWork.CommitAsync();

            return CustomResult<AccountTransaction>.Success(transaction);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole(ex.Message);

            return CustomResult<AccountTransaction>.Failure(CustomError.Failure("An error occurred during the transaction."));
        }
    }


    public async Task<CustomResult<bool>> WithdrawMoneyAsync(WithdrawMoneyDto withdrawMoneyDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            if (withdrawMoneyDto.Amount <= 0)
            {
                return CustomResult<bool>.Failure(new CustomError("AmountLessOrEqualZero", "Amount must be greater than 0."));
            }
            var validated = await bankCardService.ValidateCardAsync(withdrawMoneyDto.CardNumber,withdrawMoneyDto.Pin);
            if (!validated.IsSuccess)
            {
                return  CustomResult<bool>.Failure(validated.Error);
            }
            var balance = await unitOfWork.BankCardRepository.GetBalanceAsync(withdrawMoneyDto.CardNumber);
            if (balance < withdrawMoneyDto.Amount)
            {
                return CustomResult<bool>.Failure(new CustomError("NotEnoughBalance","Not enough balance."));
            }

            var bankAccount = await unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawMoneyDto.CardNumber);
            if (bankAccount == null){
                return CustomResult<bool>.Failure(CustomError.NotFound("Bank account not found."));
            }
            
            var newBalance = balance - withdrawMoneyDto.Amount;
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
            return CustomResult<bool>.Success(true);  
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogErrorInConsole($"Error in WithdrawMoneyAsync: {ex}");
            return CustomResult<bool>.Failure(CustomError.Failure("An error occurred during the transaction."));
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
using BankingSystem.Core.DTO;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class AccountTransactionService(IUnitOfWork unitOfWork, IExchangeRateApi exchangeRateApi) : IAccountTransactionService
{
    public async Task<string> TransactionBetweenAccountsAsync(TransactionDto transactionDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var fromAccount = await unitOfWork.AccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId);
            var toAccount = await unitOfWork.AccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId);

            var transaction = new AccountTransaction
            {
                FromAccountId = transactionDto.FromAccountId,
                ToAccountId = transactionDto.ToAccountId,
                Currency = fromAccount.Currency,
                Amount = transactionDto.Amount,
                TransactionDate = DateTime.Now
            };

            if (fromAccount.PersonId != toAccount.PersonId)
            {
                var transactionFee = transaction.Amount * 0.01m + 0.5m;
                if ((transaction.Amount + transactionFee) > fromAccount.Balance)
                {
                    return "The transaction was failed. You don't have enough money";
                }
                fromAccount.Balance -= transaction.Amount + transactionFee;
                transaction.Amount = await ConvertCurrencyAsync(transaction.Amount, fromAccount.Currency, toAccount.Currency);
                toAccount.Balance += transaction.Amount;
                await unitOfWork.AccountRepository.UpdateAccountAsync(fromAccount);
                await unitOfWork.AccountRepository.UpdateAccountAsync(toAccount);
                await unitOfWork.TransactionRepository.AddAccountTransactionAsync(transaction);

                await unitOfWork.CommitAsync();
                return "The transaction was completed successfully.";
            }

            if (transaction.Amount > fromAccount.Balance)
            {
                return "The transaction was failed. You don't have enough money";
            }

            fromAccount.Balance -= transaction.Amount;
            transaction.Amount = await ConvertCurrencyAsync(transaction.Amount, fromAccount.Currency, toAccount.Currency);
            toAccount.Balance += transaction.Amount;
            await unitOfWork.AccountRepository.UpdateAccountAsync(fromAccount);
            await unitOfWork.AccountRepository.UpdateAccountAsync(toAccount);
            await unitOfWork.TransactionRepository.AddAccountTransactionAsync(transaction);

            await unitOfWork.CommitAsync();
            return "The transaction was completed successfully.";
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync();
            return "The transaction was failed.";
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

using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.AccountTransaction;

namespace BankingSystem.Core.Services;

public class AccountTransactionService(
    IUnitOfWork unitOfWork,
    IExchangeService exchangeService,
    ILoggerService loggerService) : IAccountTransactionService
{
   public async Task<Result<AccountTransfer>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId, CancellationToken cancellationToken)
    {
        try
        {
            if (transactionDto.FromAccountId == transactionDto.ToAccountId)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation("It is not possible to make a transaction between the same accounts."));
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var fromAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId, cancellationToken);
            var toAccount = await unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId, cancellationToken);

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

            toAccount.Balance += await exchangeService.ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency);

            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(fromAccount, cancellationToken);
            await unitOfWork.BankAccountRepository.UpdateBalanceAsync(toAccount, cancellationToken);
            await unitOfWork.BankTransactionRepository.AddAccountTransferAsync(transaction, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Result<AccountTransfer>.Success(transaction);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            loggerService.LogError($"Transaction failed: {ex.Message}, StackTrace: {ex.StackTrace}, Transaction Data: {transactionDto}");

            return Result<AccountTransfer>.Failure(CustomError.Failure("An error occurred during the transaction."));
        }
    }
}
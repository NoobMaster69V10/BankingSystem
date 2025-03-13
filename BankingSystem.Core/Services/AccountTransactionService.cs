using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.Result;

namespace BankingSystem.Core.Services;

public class AccountTransactionService : IAccountTransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExchangeService _exchangeService;
    private readonly ILoggerService _loggerService;

    public AccountTransactionService(IUnitOfWork unitOfWork, IExchangeService exchangeService, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _exchangeService = exchangeService;
        _loggerService = loggerService;
    }


   public async Task<Result<AccountTransfer>> TransactionBetweenAccountsAsync(AccountTransactionDto transactionDto, string userId, CancellationToken cancellationToken)
    {
        try
        {

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            var fromAccount = await _unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.FromAccountId, cancellationToken);
            if (fromAccount is null)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation($"Bank account with  '{transactionDto.FromAccountId}' not found!"));
            }
            if (fromAccount.PersonId != userId)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation("You don't have permission to make transactions from this account."));
            }
            var toAccount = await _unitOfWork.BankAccountRepository.GetAccountByIdAsync(transactionDto.ToAccountId, cancellationToken);
            if (toAccount is null)
            {
                return Result<AccountTransfer>.Failure(CustomError.Validation($"Bank account with id '{transactionDto.ToAccountId}' not found!"));
            }
            var transactionFee = 0m;
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

            toAccount.Balance += await _exchangeService.ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency);
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(fromAccount, cancellationToken);
            await _unitOfWork.BankAccountRepository.UpdateBalanceAsync(toAccount, cancellationToken);
            await _unitOfWork.BankTransactionRepository.AddAccountTransferAsync(transaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<AccountTransfer>.Success(transaction);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _loggerService.LogError($"Transaction failed: {ex.Message}, StackTrace: {ex.StackTrace}, Transaction Data: {transactionDto}");

            return Result<AccountTransfer>.Failure(CustomError.Failure("An error occurred during the transaction."));
        }
    }
}
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;

namespace BankingSystem.Core.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggerService _loggerService;

    public BankAccountService(IUnitOfWork unitOfWork, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _loggerService = loggerService;
    }

    public async Task<Result<BankAccount>> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bankAccount =
                await _unitOfWork.BankAccountRepository.GetAccountByIbanAsync(bankAccountRegisterDto.Iban,
                    cancellationToken);
            if (bankAccount != null)
            {
                return Result<BankAccount>.Failure(CustomError.Validation("Bank account already exists."));
            }

            var person =
                await _unitOfWork.PersonRepository.GetByUsernameAsync(bankAccountRegisterDto.Username,
                    cancellationToken);
            if (person == null)
            {
                return Result<BankAccount>.Failure(CustomError.NotFound("User not found."));
            }

            var account = new BankAccount
            {
                Iban = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await _unitOfWork.BankAccountRepository.AddBankAccountAsync(account, cancellationToken);

            return Result<BankAccount>.Success(account);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Error in CreateBankAccountAsync: {ex}");
            return Result<BankAccount>.Failure(CustomError.Failure("Account could not be created."));
        }
    }

    public async Task<Result<AccountRemovalResponse>> RemoveBankAccountAsync(string iban,string userId,CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.BankAccountRepository.GetAccountByIbanAsync(iban, cancellationToken);
        if (account == null)
        {
            return Result<AccountRemovalResponse>.Failure(CustomError.Failure("Account Doesn't Exist."));
        }
        if (account.PersonId != userId)
        {
            return Result<AccountRemovalResponse>.Failure(CustomError.Failure("You don't have permission to remove this account."));
        }
        try
        {
            await _unitOfWork.BankAccountRepository.RemoveBankAccountAsync(iban, cancellationToken);
            var response = new AccountRemovalResponse
            {
                Iban = iban,
                Message = "Account removed successfully."
            };
            return Result<AccountRemovalResponse>.Success(response);
        }
        catch (Exception e)
        {
            _loggerService.LogError($"Error in RemoveBankAccountAsync: {e}");
            return Result<AccountRemovalResponse>.Failure(CustomError.Failure("Account could not be removed."));
        }
    }
}
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BankingSystem.Core.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggerService _loggerService;
    private readonly UserManager<IdentityPerson> _userManager;

    public BankAccountService(IUnitOfWork unitOfWork, ILoggerService loggerService, UserManager<IdentityPerson> userManager)
    {
        _unitOfWork = unitOfWork;
        _loggerService = loggerService;
        _userManager = userManager;
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

            var person = await _userManager.FindByNameAsync(bankAccountRegisterDto.Username);
            if (person == null)
            {
                return Result<BankAccount>.Failure(CustomError.NotFound("User not found."));
            }
            var roles = await _userManager.GetRolesAsync(person);
            if (roles.Contains(Role.Operator.ToString()) || roles.Contains(Role.Manager.ToString()))
            {
                await _userManager.AddToRoleAsync(person, Role.User.ToString());
            }
            var account = new BankAccount
            {
                Iban = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person.Id,
                Currency = bankAccountRegisterDto.Currency
            };
            await _unitOfWork.BankAccountRepository.AddBankAccountAsync(account, cancellationToken);

            var addedAccount =
                await _unitOfWork.BankAccountRepository.GetAccountByIbanAsync(bankAccountRegisterDto.Iban,
                    cancellationToken);
            return Result<BankAccount>.Success(addedAccount!);
        }
        catch (Exception ex)
        {
            _loggerService.LogError($"Error in CreateBankAccountAsync: {ex}");
            return Result<BankAccount>.Failure(CustomError.Failure("Account could not be created."));
        }
    }

    public async Task<Result<AccountRemovalResponse>> RemoveBankAccountAsync(BankAccountRemovalDto bankAccountRemovalDto,CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.BankAccountRepository.GetAccountByIbanAsync(bankAccountRemovalDto.Iban!, cancellationToken);
        if (account is null || account.PersonId != bankAccountRemovalDto.PersonId)
        {
            return Result<AccountRemovalResponse>.Failure(CustomError.Failure("Account not found."));
        }
        try
        {
            await _unitOfWork.BankAccountRepository.RemoveBankAccountAsync(bankAccountRemovalDto.Iban!, cancellationToken);
            var response = new AccountRemovalResponse
            {
                Iban = bankAccountRemovalDto.Iban!,
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
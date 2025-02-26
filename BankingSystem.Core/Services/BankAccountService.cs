using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankAccountService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankAccountService
{
    public async Task<CustomResult<BankAccount>> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto)
    {
        try
        {
            var bankAccount = await unitOfWork.BankAccountRepository.GetAccountByIbanAsync(bankAccountRegisterDto.Iban);
            if (bankAccount != null)
            {
                return CustomResult<BankAccount>.Failure(CustomError.Validation("Bank account already exists."));
            }

            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankAccountRegisterDto.Username);
            if (person == null)
            {
                return CustomResult<BankAccount>.Failure(CustomError.NotFound("User not found."));
            }

            var account = new BankAccount
            {
                IBAN = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await unitOfWork.BankAccountRepository.CreateAccountAsync(account);

            return CustomResult<BankAccount>.Success(account);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole($"Error in CreateBankAccountAsync: {ex}");
            return CustomResult<BankAccount>.Failure(CustomError.Failure("Account could not be created."));
        }
    }
}
using BankingSystem.Core.DTO;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankAccountService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankAccountService
{
    public async Task<bool> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto)
    {
        try
        {
            var person = await unitOfWork.PersonRepository.GetPersonByIdAsync(bankAccountRegisterDto.Username);

            var bankAccount = new BankAccount
            {
                IBAN = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person!.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await unitOfWork.AccountRepository.CreateAccountAsync(bankAccount);

            return true;
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return false;
        }
    }
}
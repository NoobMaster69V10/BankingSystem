using BankingSystem.Core.DTO;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankAccountService(IUnitOfWork unitOfWork) : IBankAccountService
{
    public async Task<bool> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var person = await unitOfWork.PersonRepository.GetUserByUsernameAsync(bankAccountRegisterDto.Username);

            var bankAccount = new BankAccount
            {
                IBAN = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person!.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await unitOfWork.AccountRepository.CreateAccountAsync(bankAccount);

            await unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}
using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;
using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;

namespace BankingSystem.Core.Services;

public class AccountService(IAccountRepository accountRepo) : IAccountService
{
    public async Task CreateAccountAsync(BankAccountRegisterDto bankAccountRegisterDto, string userId)
    {
        var bankAccount = new BankAccount
        {
            IBAN = bankAccountRegisterDto.IBAN,
            Balance = bankAccountRegisterDto.Balance,
            UserId = userId,
            Currency = bankAccountRegisterDto.Currency
        };

        await accountRepo.CreateAccountAsync(bankAccount);
    }
}
using BankingSystem.Core.DTO;

namespace BankingSystem.Core.ServiceContracts;

public interface IAccountService
{
    Task CreateAccountAsync(BankAccountRegisterDto bankAccountRegisterDto, string userId);
}
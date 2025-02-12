using BankingSystem.Core.DTO;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankAccountService
{
    Task<bool> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto);
}
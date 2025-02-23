using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankAccountService
{
    Task<CustomResult<BankAccount>> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto);
}
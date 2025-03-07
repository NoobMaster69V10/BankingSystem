using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankAccountService
{
    Task<Result<BankAccount>> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto, CancellationToken cancellationToken);
}
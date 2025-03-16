using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankAccountService
{
    Task<Result<BankAccount>> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto, CancellationToken cancellationToken = default);
    Task<Result<AccountRemovalResponse>> RemoveBankAccountAsync(BankAccountRemovalDto bankAccountRemovalDto, CancellationToken cancellationToken);
}
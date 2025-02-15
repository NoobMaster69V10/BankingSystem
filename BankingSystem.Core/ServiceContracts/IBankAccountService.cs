using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankAccountService
{
    Task<ApiResponse> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto);
}
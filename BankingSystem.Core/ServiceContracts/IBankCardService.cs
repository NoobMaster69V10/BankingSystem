using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankCardService
{
    Task<ApiResponse> ValidateCardAsync(string cardNumber,string pinCode);
    Task<ApiResponse> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto);
}
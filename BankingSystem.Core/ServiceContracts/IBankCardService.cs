using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankCardService
{
    Task<CustomResult<bool>> ValidateCardAsync(string cardNumber,string pinCode);
    Task<CustomResult<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto);
}
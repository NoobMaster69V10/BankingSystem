using BankingSystem.Core.DTO;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankCardService
{
    Task<bool> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto);
}
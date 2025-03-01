using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankCardService
{
    Task<Result<bool>> ValidateCardAsync(string cardNumber,string pinCode);
    Task<Result<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto);
}
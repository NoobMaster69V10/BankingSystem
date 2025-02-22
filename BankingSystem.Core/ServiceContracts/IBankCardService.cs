using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankCardService
{
    Task<Result> ValidateCardAsync(string cardNumber,string pinCode);
    Task<ResultT<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto);
}
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IBankCardService
{
    Task<Result<bool>> ValidateCardAsync(string cardNumber,string pinCode, CancellationToken cancellationToken = default);
    Task<Result<BankCard>> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto, CancellationToken cancellationToken = default);
    Task<Result<CardRemovalResponse>> RemoveBankCardAsync(BankCardActiveDto bankCardActiveDto,CancellationToken cancellationToken = default);
    Task<Result<string>> DeactivateBankCardAsync(BankCardActiveDto bankCardActiveDto,string userId,CancellationToken cancellationToken);
    Task<Result<string>> ActivateBankCardAsync(BankCardActiveDto bankCardActiveDto, string userId, CancellationToken cancellationToken);
}
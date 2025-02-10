using BankingSystem.Core.DTO;

namespace BankingSystem.Core.ServiceContracts;

public interface ICardService
{
    Task CreateCardAsync(BankCardRegisterDto bankCardRegisterDto, string userId);
}
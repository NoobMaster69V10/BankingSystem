using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;
using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;

namespace BankingSystem.Core.Services;

public class CardService(ICardRepository cardRepo) : ICardService
{
    public async Task CreateCardAsync(BankCardRegisterDto bankCardRegisterDto, string userId)
    {
        var card = new Card
        {
            CardNumber = bankCardRegisterDto.CardNumber,
            CVV = bankCardRegisterDto.CVV,
            PinCode = bankCardRegisterDto.PinCode,
            ExpirationDate = bankCardRegisterDto.ExpirationDate,
            UserId = userId,
            Name = bankCardRegisterDto.Name,
            Lastname = bankCardRegisterDto.Lastname
        };

        await cardRepo.CreateCardAsync(card);
    }
}
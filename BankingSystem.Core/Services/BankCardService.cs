using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankCardService
{
    public async Task<bool> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            var person = await unitOfWork.PersonRepository.GetUserByUsernameAsync(bankCardRegisterDto.Username);

            var card = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = bankCardRegisterDto.Cvv,
                PinCode = bankCardRegisterDto.PinCode,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                PersonId = person!.PersonId,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname
            };

            await unitOfWork.BankCardRepository.CreateCardAsync(card);
            return true;
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return false;
        }
    }
}
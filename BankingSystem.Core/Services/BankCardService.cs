using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankCardService(IUnitOfWork unitOfWork) : IBankCardService
{
    public async Task<bool> CreateBankCardAsync(BankCardRegisterDto bankCardRegisterDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var person = await unitOfWork.UserRepository.GetUserByUsernameAsync(bankCardRegisterDto.Username);

            var card = new BankCard
            {
                CardNumber = bankCardRegisterDto.CardNumber,
                Cvv = bankCardRegisterDto.Cvv,
                PinCode = bankCardRegisterDto.PinCode,
                ExpirationDate = bankCardRegisterDto.ExpirationDate,
                PersonId = person!.Id,
                Firstname = bankCardRegisterDto.Firstname,
                Lastname = bankCardRegisterDto.Lastname
            };

            await unitOfWork.BankCardRepository.CreateCardAsync(card);
            await unitOfWork.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}
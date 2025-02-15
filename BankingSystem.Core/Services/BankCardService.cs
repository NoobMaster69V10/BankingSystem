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

            if (person is null)
                throw new Exception("Person not found, username is incorrect");

            if (person.BankAccounts.Count(bc => bc.BankAccountId == bankCardRegisterDto.BankAccountId) > 0)
            {
                var card = new BankCard
                {
                    CardNumber = bankCardRegisterDto.CardNumber,
                    Cvv = bankCardRegisterDto.Cvv,
                    PinCode = bankCardRegisterDto.PinCode,
                    ExpirationDate = bankCardRegisterDto.ExpirationDate,
                    Firstname = bankCardRegisterDto.Firstname,
                    Lastname = bankCardRegisterDto.Lastname,
                    AccountId = bankCardRegisterDto.BankAccountId
                };

                await unitOfWork.BankCardRepository.CreateCardAsync(card);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return false;
        }
    }
}
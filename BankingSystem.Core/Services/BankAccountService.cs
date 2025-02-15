using System.Net;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class BankAccountService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankAccountService
{
    public async Task<ApiResponse> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto)
    {
        try
        {
            var person = await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankAccountRegisterDto.Username);

            var bankAccount = new BankAccount
            {
                IBAN = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person!.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await unitOfWork.BankAccountRepository.CreateAccountAsync(bankAccount);

            return new ApiResponse
            {
                StatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.ToString());
            return new ApiResponse
            {
                IsSuccess = false,
                ErrorMessages = [ "Error account can not be added!" ]
            };
        }
    }
}
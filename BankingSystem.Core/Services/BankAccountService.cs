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
            var bankAccount = await unitOfWork.BankAccountRepository.GetAccountByIbanAsync(bankAccountRegisterDto.Iban);
            if (bankAccount != null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Bank account already exists"]
                };
            }

            var person =
                await unitOfWork.PersonRepository.GetPersonByUsernameAsync(bankAccountRegisterDto.Username);
            if (person == null)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["User not found."]
                };
            }

            var account = new BankAccount
            {
                IBAN = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await unitOfWork.BankAccountRepository.CreateAccountAsync(account);

            return new ApiResponse
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true,
                Result = new
                {
                    Message = "Bank account created successfully.",
                    AccountId = account.Id,
                    IBAN = account.IBAN,
                    Balance = account.Balance,
                    Currency = account.Currency
                }
            };
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole($"Error in CreateBankAccountAsync: {ex}");
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = ["Error: Account could not be created."]
            };
        }
    }
}
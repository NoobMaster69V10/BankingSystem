using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class PersonService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IPersonService
{
    public async Task<CustomResult<Person>> GetPersonById(string personId)
    {
        try
        {
            var person = await unitOfWork.PersonRepository.GetPersonByIdAsync(personId);

            if (string.IsNullOrEmpty(personId) || person is null)
            {
                return CustomResult<Person>.Failure(CustomError.RecordNotFound("User not found"));
            }
           
            return CustomResult<Person>.Success(person);
        }
        catch (Exception ex)
        {
            loggerService.LogErrorInConsole(ex.Message);
            return CustomResult<Person>.Failure(CustomError.ServerError("Error occurred while getting person"));
        }
    }
}

using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class PersonService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IPersonService
{
    public async Task<Result<Person>> GetPersonById(string personId)
    {
        try
        {
            var person = await unitOfWork.PersonRepository.GetByIdAsync(personId);

            if (string.IsNullOrEmpty(personId) || person is null)
            {
                return Result<Person>.Failure(CustomError.NotFound("User not found"));
            }
           
            return Result<Person>.Success(person);
        }
        catch (Exception ex)
        {
            loggerService.LogError(ex.Message);
            return Result<Person>.Failure(CustomError.Failure("Error occurred while getting person"));
        }
    }
}

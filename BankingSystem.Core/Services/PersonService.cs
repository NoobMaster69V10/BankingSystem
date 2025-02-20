using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class PersonService(IUnitOfWork unitOfWork) : IPersonService
{
    public async Task<AdvancedApiResponse<Person>> GetPersonById(string personId)
    {
        var result = await unitOfWork.PersonRepository.GetPersonByIdAsync(personId);

        var test = 2;
        if (string.IsNullOrEmpty(personId) || result is null)
        {
            throw new Exception("User not found");
        }
       
        return AdvancedApiResponse<Person>.SuccessResponse(result);
    }
}

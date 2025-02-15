using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;

namespace BankingSystem.Core.Services;

public class PersonService(IUnitOfWork unitOfWork) : IPersonService
{
    public async Task<Person?> GetPersonById(string id)
    {
        return await unitOfWork.PersonRepository.GetPersonByIdAsync(id);
    }
}

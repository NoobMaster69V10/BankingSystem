using BankingSystem.Core.DTO.Response;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonService
{
    Task<AdvancedApiResponse<Person>> GetPersonById(string id);
}
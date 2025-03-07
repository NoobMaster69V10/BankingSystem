using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonService
{
    Task<Result<Person>> GetPersonById(string id, CancellationToken cancellationToken = default);
}
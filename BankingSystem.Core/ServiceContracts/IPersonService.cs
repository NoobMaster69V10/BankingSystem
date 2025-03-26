using BankingSystem.Core.Result;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonService
{
    Task<Result<Person>> GetPersonByIdAsync(string id, CancellationToken cancellationToken = default);
}
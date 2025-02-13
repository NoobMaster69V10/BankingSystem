using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonService
{
    Task<Person?> GetPersonById(string id);
}
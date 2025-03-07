using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IJwtTokenGeneratorService
{
    Task<string> GenerateJwtToken(IdentityPerson person);
}
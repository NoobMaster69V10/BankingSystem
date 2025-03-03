using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IJwtTokenGenerator
{
    Task<string> GenerateJwtToken(IdentityPerson person);
}
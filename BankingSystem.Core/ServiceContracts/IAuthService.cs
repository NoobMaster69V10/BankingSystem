using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IAuthService
{
    public Task<string> GenerateJwtToken(IdentityPerson user);
}

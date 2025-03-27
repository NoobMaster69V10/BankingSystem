using System.Security.Claims;

namespace BankingSystem.Core.ServiceContracts;

public interface IAuthTokenGeneratorService
{
    Task<string> GenerateAccessTokenAsync(IdentityPerson person);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
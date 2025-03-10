using System.Security.Claims;
using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IJwtTokenGeneratorService
{
    Task<string> GenerateAccessTokenAsync(IdentityPerson person);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
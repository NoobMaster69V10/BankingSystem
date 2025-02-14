using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<string?> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<bool> RegisterPersonAsync(PersonRegisterDto registerDto);
    public Task<string> GenerateJwtToken(IdentityPerson user);
}

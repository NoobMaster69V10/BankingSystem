using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<AuthenticationResponse> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<bool> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<AuthenticationResponse> GenerateJwtToken(IdentityPerson user);
}

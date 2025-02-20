using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<AdvancedApiResponse<string>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<AdvancedApiResponse<string>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<string> GenerateJwtToken(IdentityPerson user);
}

using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<CustomResult<string>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<CustomResult<IdentityPerson>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<string> GenerateJwtToken(IdentityPerson user);
}

using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Identity;
using BankingSystem.Domain.Entities;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<CustomResult<string>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<CustomResult<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<CustomResult<PersonRegisterDto>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<string> GenerateJwtToken(IdentityPerson user);
    Task<CustomResult<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}

using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Identity;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<Result<string>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<Result<PersonRegisterDto>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<string> GenerateJwtToken(IdentityPerson user);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}

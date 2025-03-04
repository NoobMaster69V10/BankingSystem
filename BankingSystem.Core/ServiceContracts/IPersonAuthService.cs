using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<Result<string>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<Result<RegisterResponse>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}

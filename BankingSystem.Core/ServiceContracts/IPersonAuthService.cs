using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Response;
using FluentEmail.Core;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<Result<AuthenticatedResponse>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<Result<RegisterResponse>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<Result<string>> EmailConfirmationAsync(string token,string email);
}

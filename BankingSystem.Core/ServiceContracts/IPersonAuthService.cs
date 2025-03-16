using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.RefreshToken;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<Result<AuthenticatedResponse>> AuthenticationPersonAsync(PersonLoginDto loginDto);
    Task<Result<AuthenticatedResponse>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<Result<RegisterResponse>> RegisterPersonAsync(PersonRegisterDto registerDto);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<Result<string>> EmailConfirmationAsync(string token, string email);
}

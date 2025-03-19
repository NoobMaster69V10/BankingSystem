using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.RefreshToken;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;

namespace BankingSystem.Core.ServiceContracts;

public interface IPersonAuthService
{
    Task<Result<AuthenticatedResponse>> AuthenticationPersonAsync(PersonLoginDto loginDto, CancellationToken cancellationToken = default);
    Task<Result<AuthenticatedResponse>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken = default);
    Task<Result<RegisterResponse>> RegisterPersonAsync(PersonRegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default);
    Task<Result<string>> EmailConfirmationAsync(string token, string email, CancellationToken cancellationToken = default);
}

using System.Security.Claims;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.RefreshToken;
using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BankingSystem.Tests.Services;

public class PersonAuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<UserManager<IdentityPerson>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IAuthTokenGeneratorService> _tokenGeneratorMock = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly Mock<ILoggerService> _loggerServiceMock = new();
    private readonly PersonAuthService _authService;

    public PersonAuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityPerson>>(
            new Mock<IUserStore<IdentityPerson>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object, null!, null!, null!, null!);

        _authService = new PersonAuthService(
            _unitOfWorkMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _emailServiceMock.Object,
            _tokenGeneratorMock.Object,
            _contextAccessorMock.Object,
            _loggerServiceMock.Object);
    }

    [Fact]
    public async Task AuthenticationPersonAsync_UserNotFound_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityPerson)null!);

        var result = await _authService.AuthenticationPersonAsync(new PersonLoginDto { Email = "test@test.com", Password = "password" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email", result.Error!.Message);
    }

    [Fact]
    public async Task RegisterPersonAsync_SuccessfulRegistration_ReturnsSuccess()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityPerson>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityPerson>()))
            .ReturnsAsync("confirmation_token");

        var result = await _authService.RegisterPersonAsync(new PersonRegisterDto { Email = "test@test.com", Password = "Password123!" });

        Assert.True(result.IsSuccess);
        Assert.Equal("test@test.com", result.Value!.Email);
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityPerson)null!);

        var result = await _authService.ForgotPasswordAsync(new ForgotPasswordDto { Email = "test@test.com", ClientUri = "https://example.com" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email", result.Error!.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new IdentityPerson());
        _userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<IdentityPerson>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await _authService.ResetPasswordAsync(new ResetPasswordDto { Email = "test@test.com", Token = "invalid_token", NewPassword = "NewPassword123!" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid or expired token", result.Error!.Message);
    }

    [Fact]
    public async Task EmailConfirmationAsync_InvalidToken_ReturnsFailure()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new IdentityPerson());
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(It.IsAny<IdentityPerson>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Invalid", Description = "Invalid request" }));

        var result = await _authService.EmailConfirmationAsync("invalid_token", "test@test.com");

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email confirmation request", result.Error!.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsFailure()
    {
        _tokenGeneratorMock.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>()))
            .Returns((ClaimsPrincipal)null!);

        var result = await _authService.RefreshTokenAsync(new RefreshTokenDto { Token = "expired_token", RefreshToken = "refresh_token" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Error occurred while refreshing token", result.Error!.Message);
    }
}
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BankingSystem.Tests.Services;

public class PersonAuthServiceTests
{
    private readonly IPersonAuthService _personAuthService;
    private readonly Mock<UserManager<IdentityPerson>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    
    public PersonAuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Key", "d556da2a430031e0485fb62d9dc8e469e845b5079fe68468c33bd896b93fc2124347d8fae78fb20fcc22dfcf21b50236003fe4930ec9961b3c2ed048e4bcb33cb24b49647b05710b3063b02ee3048c8ef1453e9b9edef9406614f3fcb12c42fb7ad4735f64cf9cba3ed6bf7cec0b26e3c5505ac20f6d27291505f37d97eb5dfe7076ee50a39b8ef0c99852af78f112654021ce586ebeae5ac441530b683892c95066c53f7209b39decb337d6b6b8b7871b70d3bd35bb12110e4b2579bdb35278e090493bcb23ead8392bdb1afc0ded4aea25d2a4f69e3a8ddf23619fe2bc7c2d64c07c5dc0b6fdeaca12fa56ca5ee329c1654088526a80d34c04cc097d45938b" },
            { "Jwt:Issuer", "http://localhost:7221" },
            { "Jwt:Audience", "http://localhost:4200" },
            { "Jwt:EXPIRATION_MINUTES", "60" }
        };
        new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
        _userManagerMock = new Mock<UserManager<IdentityPerson>>(
            Mock.Of<IUserStore<IdentityPerson>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null!, null!, null!, null!);
        Mock<ILoggerService> loggerServiceMock = new();
        Mock<IEmailService> emailServiceMock = new();
        Mock<IAuthTokenGeneratorService> jwtTokenGeneratorMock = new();
        Mock<IUnitOfWork> unitOfWorkMock = new();

        _personAuthService = new PersonAuthService(
            unitOfWorkMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            emailServiceMock.Object,
            jwtTokenGeneratorMock.Object,
            loggerServiceMock.Object
        );
    }

    [Fact]
    public async Task AuthenticationPersonAsync_ShouldReturnFailure_WhenPersonNotFound()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityPerson)null!);

        var result = await _personAuthService.AuthenticationPersonAsync(new PersonLoginDto());

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email", result.Error!.Message);
    }

    [Fact]
    public async Task AuthenticationPersonAsync_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new IdentityPerson());
        _userManagerMock.Setup(x => x.CheckPasswordAsync(new IdentityPerson{ UserName = "test@gmail.com" }, "pass123")).ReturnsAsync(false);

        var result = await _personAuthService.AuthenticationPersonAsync(new PersonLoginDto { Email = "test@gmail.com", Password = "pass123" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid password", result.Error!.Message);
    }

    [Fact]
    public async Task AuthenticationPersonAsync_ShouldReturnSuccess_WhenIsValid()
    {
        var user = new IdentityPerson
        {
            UserName = "test@gmail.com"
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@gmail.com")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "pass123")).ReturnsAsync(true);

        var result = await _personAuthService.AuthenticationPersonAsync(new PersonLoginDto { Email = "test@gmail.com", Password = "pass123" });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterPersonAsync_ShouldReturnFailure_WhenUserCannotBeCreated()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityPerson>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

        var result = await _personAuthService.RegisterPersonAsync(new PersonRegisterDto());
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterPersonAsync_ShouldReturnFailure_WhenRoleNotExists()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityPerson>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        var result = await _personAuthService.RegisterPersonAsync(new PersonRegisterDto());
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RegisterPersonAsync_ShouldReturnSuccess_WhenIsValid()
    {
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityPerson>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        var result = await _personAuthService.RegisterPersonAsync(new PersonRegisterDto());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnFailure_WhenUserIsNull()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityPerson)null!);

        var result = await _personAuthService.ForgotPasswordAsync(new ForgotPasswordDto());
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email", result.Error!.Message);
    }

}

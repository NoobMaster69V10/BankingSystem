using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankAccountServiceTests
{
    private readonly IBankAccountService _bankAccountService;
    private readonly Mock<UserManager<IdentityPerson>> _userManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;


    public BankAccountServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ILoggerService> loggerServiceMock = new();
        _userManagerMock = new Mock<UserManager<IdentityPerson>>(
            new Mock<IUserStore<IdentityPerson>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _bankAccountService = new BankAccountService(
            _unitOfWorkMock.Object,
            loggerServiceMock.Object,
            _userManagerMock.Object);
    }

    #region CreateBankAccountAsync Tests

    [Fact]
    public async Task CreateBankAccountAsync_ShouldReturnFailure_WhenBankAccountIsNotNull()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync("GE00AS002313213211", default)).ReturnsAsync(new BankAccount());
        var bankAccountRegisterDto = new BankAccountRegisterDto { Username = "test@gmail.com", Balance = 2000, Currency = Currency.USD, Iban = "GE00AS002313213211" };

        var result = await _bankAccountService.CreateBankAccountAsync(bankAccountRegisterDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account already exists.", result.Error!.Message);
    }
    [Fact]
    public async Task CreateBankAccountAsync_ShouldReturnFailure_WhenPersonIsNull()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync("GE00AS002313213211", default)).ReturnsAsync((BankAccount)null!);
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("test@gmail.com", default)).ReturnsAsync((Person)null!);

        var bankAccountRegisterDto = new BankAccountRegisterDto { Username = "test@gmail.com", Balance = 2000, Currency = Currency.USD, Iban = "GE00AS002313213211" };
        var result = await _bankAccountService.CreateBankAccountAsync(bankAccountRegisterDto);
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateBankAccountAsync_ShouldReturnWhen_WhenValid()
    {
        var person = new IdentityPerson();

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync("GE00AS002313213211", default)).ReturnsAsync((BankAccount)null!);
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("test@gmail.com", default)).ReturnsAsync(new Person());
        _userManagerMock.Setup(u => u.FindByNameAsync("test@gmail.com")).ReturnsAsync(person);
        _userManagerMock.Setup(u => u.GetRolesAsync(person)).ReturnsAsync(new List<string>());
        var bankAccountRegisterDto = new BankAccountRegisterDto { Username = "test@gmail.com", Balance = 2000, Currency = Currency.USD, Iban = "GE00AS002313213211" };
        var result = await _bankAccountService.CreateBankAccountAsync(bankAccountRegisterDto);
        Assert.True(result.IsSuccess);
    }
    #endregion

    #region RemoveBankAccountAsync Tests

    [Fact]
    public async Task RemoveBankAccountAsync_ShouldReturnFailure_WhenAccountIsNull()
    {
        var bankAccountRemovalDto = new BankAccountRemovalDto { Iban = "12345", PersonId = "user1" };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync(bankAccountRemovalDto.Iban, default))
            .ReturnsAsync((BankAccount)null!);

        var result = await _bankAccountService.RemoveBankAccountAsync(bankAccountRemovalDto);
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found.", result.Error!.Message);
    }


    [Fact]
    public async Task RemoveBankAccountAsync_ShouldReturnSuccess_WhenAccountDeleted()
    {
        var bankAccountRemovalDto = new BankAccountRemovalDto { Iban = "12345", PersonId = "user" };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync(bankAccountRemovalDto.Iban, default))
            .ReturnsAsync(new BankAccount{ PersonId = "user"});

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.RemoveBankAccountAsync(bankAccountRemovalDto.Iban, default))
            .Returns(Task.CompletedTask);

        var result = await _bankAccountService.RemoveBankAccountAsync(bankAccountRemovalDto);
        Assert.True(result.IsSuccess);
    }
    #endregion

}

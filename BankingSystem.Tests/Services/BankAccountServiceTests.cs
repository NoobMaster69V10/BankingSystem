using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankAccountServiceTests
{
    private readonly IBankAccountService _bankAccountService;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;

    public BankAccountServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerServiceMock = new Mock<ILoggerService>();

        _bankAccountService = new BankAccountService(
            _unitOfWorkMock.Object,
            _loggerServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateBankAccountAsync_ShouldReturnFailure_WhenBankAccountIsNotNull()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync("GE00AS002313213211")).ReturnsAsync(new BankAccount());
        var bankAccountRegisterDto = new BankAccountRegisterDto{Username = "test@gmail.com", Balance = 2000, Currency = "USD", Iban = "GE00AS002313213211"};

        var result = await _bankAccountService.CreateBankAccountAsync(bankAccountRegisterDto);
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account already exists.", result.Error.Message);
    }
    [Fact]
    public async Task CreateBankAccountAsync_ShouldReturnFailure_WhenPersonIsNull()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync("GE00AS002313213211")).ReturnsAsync((BankAccount)null);
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("test@gmail.com")).ReturnsAsync((Person)null);
        
        var bankAccountRegisterDto = new BankAccountRegisterDto { Username = "test@gmail.com", Balance = 2000, Currency = "USD", Iban = "GE00AS002313213211" };
        var result = await _bankAccountService.CreateBankAccountAsync(bankAccountRegisterDto);
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.Error.Message);
    }

    [Fact]
    public async Task CreateBankAccountAsync_ShouldReturnWhen_WhenValid()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIbanAsync("GE00AS002313213211")).ReturnsAsync((BankAccount)null);
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("test@gmail.com")).ReturnsAsync(new Person());

        var bankAccountRegisterDto = new BankAccountRegisterDto { Username = "test@gmail.com", Balance = 2000, Currency = "USD", Iban = "GE00AS002313213211" };
        var result = await _bankAccountService.CreateBankAccountAsync(bankAccountRegisterDto);
        Assert.True(result.IsSuccess);
    }
}
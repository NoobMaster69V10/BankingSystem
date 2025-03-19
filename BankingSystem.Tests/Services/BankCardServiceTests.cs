using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankCardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IBankCardService _bankCardService;
    private readonly Mock<IHasherService> _hasherService;

    public BankCardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ILoggerService> loggerServiceMock = new();
        _hasherService = new Mock<IHasherService>();
        Mock<IEncryptionService> encryptionService = new();
        _bankCardService = new BankCardService(
            _unitOfWorkMock.Object,
            _hasherService.Object,
            encryptionService.Object,
            loggerServiceMock.Object
        );
    }

    #region CreateBankCardAsync Tests
    [Fact]
    public async Task CreateBankCardAsync_ShouldReturnFailure_WhenCardNumberExists()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardAsync("1234567890123456", default)).ReturnsAsync(new BankCard());
        var result = await _bankCardService.CreateBankCardAsync(new BankCardRegisterDto { CardNumber = "1234567890123456" });
        Assert.False(result.IsSuccess);
        Assert.Equal("A card with this number already exists. Please use a different card number.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateBankCardAsync_ShouldReturnFailure_PersonNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardAsync("1234567890121", default)).ReturnsAsync(new BankCard());
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("username", default)).ReturnsAsync((Person?)null);

        var result = await _bankCardService.CreateBankCardAsync(new BankCardRegisterDto { CardNumber = "1234567890123456", Username = "username" });
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found. Please check the username and try again.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateBankCardAsync_ShouldReturnFailure_AccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardAsync("1234567890121", default)).ReturnsAsync(new BankCard());
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("username", default)).ReturnsAsync(new Person());
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(1, default)).ReturnsAsync((BankAccount?)null);

        var result = await _bankCardService.CreateBankCardAsync(new BankCardRegisterDto { CardNumber = "1234567890123456", Username = "username", BankAccountId = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("The provided bank account does not exist. Please verify your details.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateBankCardAsync_ShouldReturnFailure_IfPersonDoNotHaveAccount()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardAsync("1234567890121", default)).ReturnsAsync(new BankCard());
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("username", default)).ReturnsAsync(new Person{BankAccounts = new List<BankAccount>()});
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(1, default)).ReturnsAsync(new BankAccount());

        var result = await _bankCardService.CreateBankCardAsync(new BankCardRegisterDto { CardNumber = "1234567890123456", Username = "username", BankAccountId = 1});
        Assert.False(result.IsSuccess);
        Assert.Equal("You do not have any linked bank accounts. Please create an account first.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateBankCardAsync_ShouldReturnFailure_IfPersonDoNotHaveEnteredAccount()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardAsync("1234567890121", default)).ReturnsAsync(new BankCard());
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("username", default)).ReturnsAsync(new Person { BankAccounts = new List<BankAccount>(){ new() {BankAccountId = 1}} });
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(2, default)).ReturnsAsync(new BankAccount());

        var result = await _bankCardService.CreateBankCardAsync(new BankCardRegisterDto { CardNumber = "1234567890123456", Username = "username", BankAccountId = 2 });
        Assert.False(result.IsSuccess);
        Assert.Equal("This bank account is not associated with your profile. Please use a valid account.", result.Error!.Message);
    }

    [Fact]
    public async Task CreateBankCardAsync_ShouldReturnSuccess_AllCorrect()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardAsync("1234567890121", default)).ReturnsAsync(new BankCard());
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByUsernameAsync("username", default)).ReturnsAsync(new Person { BankAccounts = new List<BankAccount>() { new() { BankAccountId = 1 } } });
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(1, default)).ReturnsAsync(new BankAccount{BankAccountId = 1});
        var result = await _bankCardService.CreateBankCardAsync(new BankCardRegisterDto { CardNumber = "1234567890123456", Username = "username", BankAccountId = 1, Cvv = "123", PinCode = "1234"});
        Assert.True(result.IsSuccess);
    }


    #endregion

    #region RemoveBankCardAsync Tests
    [Fact]
    public async Task RemoveBankCardAsync_ShouldReturnFailure_WhenAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync((BankAccount)null!);

        var result = await _bankCardService.RemoveBankCardAsync(new BankCardActiveDto{CardNumber = "1234567890123456" });

        Assert.False(result.IsSuccess);
        Assert.Equal("Card not found", result.Error!.Message);
    }

    [Fact]
    public async Task RemoveBankCardAsync_ShouldReturnSuccess_WhenValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync(new BankAccount{ PersonId = "1" });

        var result = await _bankCardService.RemoveBankCardAsync(new BankCardActiveDto { CardNumber = "1234567890123456", PersonId = "1" });

        Assert.True(result.IsSuccess);
    }
    #endregion

    #region DeactivateBankCardAsync Tests

    [Fact]
    public async Task DeactivateBankCardAsync_ShouldReturnFailure_WhenAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync((BankAccount)null!);

        var result = await _bankCardService.DeactivateBankCardAsync("1234567890123456", "user");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card not found", result.Error!.Message);
    }

    [Fact]
    public async Task DeactivateBankCardAsync_ShouldReturnFailure_WhenCardIsAlreadyDeactivated()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync(new BankAccount{ PersonId = "user" });

        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardStatusAsync("1234567890123456", default)).ReturnsAsync(false);

        var result = await _bankCardService.DeactivateBankCardAsync("1234567890123456", "user");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card is already deactivated", result.Error!.Message);
    }

    [Fact]
    public async Task DeactivateBankCardAsync_ShouldReturnSuccess_WhenValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync(new BankAccount { PersonId = "user" });

        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardStatusAsync("1234567890123456", default)).ReturnsAsync(true);

        var result = await _bankCardService.DeactivateBankCardAsync("1234567890123456", "user");

        Assert.True(result.IsSuccess);
    }
    #endregion

    #region ActivateBankCardAsync Tests
    [Fact]
    public async Task ActivateBankCardAsync_ShouldReturnFailure_WhenAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync((BankAccount)null!);

        var result = await _bankCardService.ActivateBankCardAsync("1234567890123456", "user");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card not found", result.Error!.Message);
    }

    [Fact]
    public async Task ActivateBankCardAsync_ShouldReturnFailure_WhenCardIsExpired()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync(new BankAccount { PersonId = "user" });
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default))
            .ReturnsAsync(("1234", DateTime.Now.AddDays(-20), "123", true));

        var result = await _bankCardService.ActivateBankCardAsync("1234567890123456", "user");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card is expired, You can't activate it. Visit Bank", result.Error!.Message);
    }

    [Fact]
    public async Task ActivateBankCardAsync_ShouldReturnSuccess_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234567890123456", default)).ReturnsAsync(new BankAccount { PersonId = "user" });
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default))
            .ReturnsAsync(("1234", DateTime.Now.AddDays(20), "123", true));

        var result = await _bankCardService.ActivateBankCardAsync("1234567890123456", "user");

        Assert.True(result.IsSuccess);
    }
    #endregion

    #region ValidateCardAsync_Tests
    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync(((string PinCode, DateTime ExpiryDate, string Cvv, bool IsActive)?)null);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card not found", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenPinCodeIsNotValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync((("1234", DateTime.Now, "123", true)));

        _hasherService.Setup(h => h.Verify("1234", "3214")).Returns(false);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Pin code does not match", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardIsExpired()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync((("1234", DateTime.Now.AddDays(-30), "123", true)));

        _hasherService.Setup(h => h.Verify("1234", "1234")).Returns(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card is expired", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync((("1234", DateTime.Now.AddDays(30), "123", true)));

        _hasherService.Setup(h => h.Verify("1234", "1234")).Returns(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.True(result.IsSuccess);
    }
    #endregion
}

using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
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

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync(((string PinCode, DateTime ExpiryDate, string Cvv)?)null);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card number not found", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenPinCodeIsNotValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync((("1234", DateTime.Now, "123")));

        _hasherService.Setup(h => h.Verify("1234", "3214")).Returns(false);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Pin code does not match", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardIsExpired()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync((("1234", DateTime.Now.AddDays(-30), "123")));

        _hasherService.Setup(h => h.Verify("1234", "1234")).Returns(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card is expired", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardSecurityDetailsAsync("1234567890123456", default)).ReturnsAsync((("1234", DateTime.Now.AddDays(30), "123")));

        _hasherService.Setup(h => h.Verify("1234", "1234")).Returns(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.True(result.IsSuccess);
    }
}


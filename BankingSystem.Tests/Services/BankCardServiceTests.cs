using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankCardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly IBankCardService _bankCardService;
    private readonly Mock<IHasherService> _hasherService;
    private readonly Mock<IEncryptionService> _encryptionService;

    public BankCardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerServiceMock = new Mock<ILoggerService>();
        _loggerServiceMock = new Mock<ILoggerService>();
        _hasherService = new Mock<IHasherService>();
        _encryptionService = new Mock<IEncryptionService>();

        _bankCardService = new BankCardService(
            _unitOfWorkMock.Object,
            _loggerServiceMock.Object,
            _hasherService.Object,
            _encryptionService.Object
        );
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardDetailsAsync("1234567890123456")).ReturnsAsync(((string PinCode, DateTime ExpiryDate, string Cvv)?)null);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card number not found", result.Error.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenPinCodeIsNotValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardDetailsAsync("1234567890123456")).ReturnsAsync((("1234", DateTime.Now, "123")));

        _hasherService.Setup(h => h.Verify("1234", "3214")).Returns(false);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Pin code does not match", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardIsExpired()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardDetailsAsync("1234567890123456")).ReturnsAsync((("1234", DateTime.Now.AddDays(-30), "123")));

        _hasherService.Setup(h => h.Verify("1234", "1234")).Returns(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card is expired", result.Error!.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetCardDetailsAsync("1234567890123456")).ReturnsAsync((("1234", DateTime.Now.AddDays(30), "123")));

        _hasherService.Setup(h => h.Verify("1234", "1234")).Returns(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.True(result.IsSuccess);
    }
}


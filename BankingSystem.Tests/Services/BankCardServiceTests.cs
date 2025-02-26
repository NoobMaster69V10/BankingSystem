using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests;

public class BankCardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly IBankCardService _bankCardService;

    public BankCardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerServiceMock = new Mock<ILoggerService>();

        _bankCardService = new BankCardService(
            _unitOfWorkMock.Object,
            _loggerServiceMock.Object
        );
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.DoesCardExistAsync("1234567890123456")).ReturnsAsync(false);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card number not found", result.Error.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenPinCodeIsNotValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.DoesCardExistAsync("1234567890123456")).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BankCardRepository.CheckPinCodeAsync("1234567890123456", "1234")).ReturnsAsync(false);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Pin code does not match", result.Error.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenCardIsExpired()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.DoesCardExistAsync("1234567890123456")).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BankCardRepository.CheckPinCodeAsync("1234567890123456", "1234")).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BankCardRepository.IsCardExpiredAsync("1234567890123456")).ReturnsAsync(true);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.False(result.IsSuccess);
        Assert.Equal("Card is expired", result.Error.Message);
    }

    [Fact]
    public async Task ValidateCardAsync_ShouldReturnFailure_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.BankCardRepository.DoesCardExistAsync("1234567890123456")).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BankCardRepository.CheckPinCodeAsync("1234567890123456", "1234")).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BankCardRepository.IsCardExpiredAsync("1234567890123456")).ReturnsAsync(false);

        var result = await _bankCardService.ValidateCardAsync("1234567890123456", "1234");

        Assert.True(result.IsSuccess);
    }
}


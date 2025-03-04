using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExchangeRateApi> _exchangeRateApiMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly BankReportService _bankReportService;

    public BankReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _exchangeRateApiMock = new Mock<IExchangeRateApi>();
        _loggerMock = new Mock<ILoggerService>();

        _bankReportService = new BankReportService(
            _unitOfWorkMock.Object,
            _exchangeRateApiMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetUserStatisticsAsync_ShouldFetchData_WhenThereIsNotException()
    {
        var now = DateTime.Now;
        var startOfThisYear = new DateTime(now.Year, 1, 1);
        var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
        var thirtyDaysAgo = now.AddDays(-30);

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(startOfThisYear))
            .ReturnsAsync(500);

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(startOfLastYear))
            .ReturnsAsync(500);

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(startOfThisYear))
            .ReturnsAsync(500);
        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(thirtyDaysAgo))
            .ReturnsAsync(500);

        var result = await _bankReportService.GetUserStatisticsAsync();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_ShouldReturnFailure_WhenException()
    {
        var now = DateTime.Now;
        var startOfThisYear = new DateTime(now.Year, 1, 1);
        var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
        var thirtyDaysAgo = now.AddDays(-30);

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(startOfThisYear))
            .ThrowsAsync(new Exception());

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(startOfLastYear))
            .ThrowsAsync(new Exception());

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(startOfThisYear))
            .ThrowsAsync(new Exception());
        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(thirtyDaysAgo))
            .ThrowsAsync(new Exception());

        var result = await _bankReportService.GetUserStatisticsAsync();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetTransactionStatisticsAsync_ShouldFetchData_WhenIsNotException()
    {
        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetTransactionCountAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(500);

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetTransactionIncomeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<Currency, decimal> { { Currency.USD, 10000m } });

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetAverageTransactionIncomeAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<Currency, decimal> { { Currency.USD, 200m } });

        var result = await _bankReportService.GetTransactionStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(500, result.Value.TransactionsLastMonth);
        _unitOfWorkMock.Verify(repo => repo.BankReportRepository.GetTransactionCountAsync(It.IsAny<DateTime>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GetTransactionStatisticsAsync_ShouldReturnFailure_WhenException()
    {
        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetTransactionCountAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception());

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetTransactionIncomeAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception());

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetAverageTransactionIncomeAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception());

        var result = await _bankReportService.GetTransactionStatisticsAsync();

        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task GetDailyTransactionsAsync_ShouldFetchData_WhenTransactionsNotEmpty()
    {
        var fakeData = new List<DailyTransactionReport>
        {
            new DailyTransactionReport { Date = DateTime.UtcNow, TotalAmount = new Dictionary<Currency, decimal> { { Currency.USD, 1000m } } }
        };

        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetDailyTransactionsAsync(It.IsAny<int>()))
            .ReturnsAsync(fakeData);

        var result = await _bankReportService.GetDailyTransactionsAsync(30);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        _unitOfWorkMock.Verify(repo => repo.BankReportRepository.GetDailyTransactionsAsync(30), Times.Once);
    }

    [Fact]
    public async Task GetAtmTransactionsStatisticsAsync_ShouldFetchData_WhenIsNotException()
    {
        var fakeData = new List<AtmTransaction>
        {
            new AtmTransaction{ Amount = 100, Currency = "USD" }
        };

        _unitOfWorkMock.Setup(repo => repo.AtmRepository.GetAllAtmTransactionsAsync())
            .ReturnsAsync(fakeData);

        _exchangeRateApiMock.Setup(api => api.GetExchangeRate("USD"))
            .ReturnsAsync(2m);

        var result = await _bankReportService.GetAtmTransactionsStatisticsAsync();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAtmTransactionsStatisticsAsync_ShouldReturnFailure_WhenException()
    {
        var fakeData = new List<AtmTransaction>
        {
            new AtmTransaction{ Amount = 100, Currency = "USD" }
        };

        _unitOfWorkMock.Setup(repo => repo.AtmRepository.GetAllAtmTransactionsAsync())
            .ThrowsAsync(new Exception());

        _exchangeRateApiMock.Setup(api => api.GetExchangeRate("USD"))
            .ThrowsAsync(new Exception());

        var result = await _bankReportService.GetAtmTransactionsStatisticsAsync();

        Assert.False(result.IsSuccess);
    }
}
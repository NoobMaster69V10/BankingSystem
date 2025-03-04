using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.Statistics;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExchangeRateApi> _exchangeRateApiMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly BankReportService _bankReportService;

    public BankReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _exchangeRateApiMock = new Mock<IExchangeRateApi>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILoggerService>();

        _bankReportService = new BankReportService(
            _unitOfWorkMock.Object,
            _exchangeRateApiMock.Object,
            _memoryCache,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetUserStatisticsAsync_ShouldReturnCachedData_WhenDataIsCached()
    {
        var expectedStats = new UserStatistics { TotalUsers = 100 };
        _memoryCache.Set("UserStatistics", expectedStats);

        var result = await _bankReportService.GetUserStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.TotalUsers);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_ShouldFetchData_WhenCacheIsEmpty()
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
    public async Task GetUserStatisticsAsync_ShouldLogError_WhenExceptionOccurs()
    {
        _unitOfWorkMock.Setup(repo => repo.BankReportRepository.GetUserCountAsync(It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("Database Error"));

        await Assert.ThrowsAsync<Exception>(() => _bankReportService.GetUserStatisticsAsync());
        _loggerMock.Verify(logger => logger.LogError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetTransactionStatisticsAsync_ShouldReturnCachedData_WhenDataIsCached()
    {
        var expectedStats = new TransactionStatistics { TransactionsLastMonth = 500 };
        _memoryCache.Set("TransactionStatistics", expectedStats);

        var result = await _bankReportService.GetTransactionStatisticsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(500, result.Value.TransactionsLastMonth);
    }

    [Fact]
    public async Task GetTransactionStatisticsAsync_ShouldFetchData_WhenCacheIsEmpty()
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
    public async Task GetDailyTransactionsAsync_ShouldReturnCachedData_WhenAvailable()
    {
        var expectedData = new List<DailyTransactionReport>
        {
            new DailyTransactionReport { Date = DateTime.UtcNow, TotalAmount = new Dictionary<Currency, decimal> { { Currency.USD, 1000m } } }
        };
        _memoryCache.Set("DailyTransactions_30", expectedData);

        var result = await _bankReportService.GetDailyTransactionsAsync(30);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task GetDailyTransactionsAsync_ShouldFetchData_WhenCacheIsEmpty()
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
}
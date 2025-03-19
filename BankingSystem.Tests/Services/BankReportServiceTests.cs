using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Infrastructure.UnitOfWork;
using Moq;
using System.Threading;

namespace BankingSystem.Tests.Services;

public class BankReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly BankReportService _bankReportService;

    public BankReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ICurrencyExchangeClient> exchangeRateApiMock = new();
        Mock<ILoggerService> loggerMock = new();

        _bankReportService = new BankReportService(
            _unitOfWorkMock.Object,
            exchangeRateApiMock.Object,
            loggerMock.Object
        );
    }

    #region GetUserStatisticsAsync Tests

    [Fact]
    public async Task GetUserStatisticsAsync_ShouldReturnFailure_WhenException()
    {
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetUserCountAsync(null, default))
            .ThrowsAsync(new Exception("Test Exception"));

        var result = await _bankReportService.GetUserStatisticsAsync();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_ShouldReturnSuccess_WhenValid()
    {
        var now = DateTime.Now;
        var startOfThisYear = new DateTime(now.Year, 1, 1);
        var startOfLastYear = new DateTime(now.Year - 1, 1, 1);
        var thirtyDaysAgo = now.AddDays(-30);

        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetUserCountAsync(null, default)).ReturnsAsync(20);
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetUserCountAsync(startOfThisYear, default)).ReturnsAsync(20);
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetUserCountAsync(startOfLastYear, default)).ReturnsAsync(20);
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetUserCountAsync(thirtyDaysAgo, default)).ReturnsAsync(20);

        var result = await _bankReportService.GetUserStatisticsAsync();
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region GetTransactionStatisticsAsync Tests

    [Fact]
    public async Task GetTransactionStatisticsAsync_ShouldReturnFailure_WhenException()
    {

        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetAverageTransactionIncomeAsync(null, default))
            .ThrowsAsync(new Exception());

        var result = await _bankReportService.GetTransactionStatisticsAsync();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetTransactionStatisticsAsync_ShouldReturnSuccess_WhenValid()
    {
        var now = DateTime.Now;
        var oneMonthAgo = now.AddMonths(-1);
        var sixMonthsAgo = now.AddMonths(-6);
        var oneYearAgo = now.AddYears(-1);

        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetTransactionCountAsync(oneMonthAgo, default)).ReturnsAsync(20);
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetTransactionCountAsync(sixMonthsAgo, default)).ReturnsAsync(20);
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetTransactionCountAsync(oneYearAgo, default)).ReturnsAsync(20);
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetTransactionIncomeAsync(oneMonthAgo, default)).ReturnsAsync(new Dictionary<Currency, decimal>());
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetTransactionIncomeAsync(sixMonthsAgo, default)).ReturnsAsync(new Dictionary<Currency, decimal>());
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetTransactionIncomeAsync(oneYearAgo, default)).ReturnsAsync(new Dictionary<Currency, decimal>());
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetAverageTransactionIncomeAsync(null, default)).ReturnsAsync(new Dictionary<Currency, decimal>());

        var result = await _bankReportService.GetTransactionStatisticsAsync();
        Assert.True(result.IsSuccess);
    }
    #endregion

    #region GetDailyTransactionsAsync Tests
    [Theory]
    [InlineData(50)]
    public async Task GetDailyTransactionsAsync_ShouldReturnSuccess_WhenValid(int days)
    {
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetDailyTransactionsAsync(days, default))
            .ReturnsAsync(new List<DailyTransactionReport>());

        var result = await _bankReportService.GetDailyTransactionsAsync();
        Assert.True(result.IsSuccess);
    }
    #endregion

    #region GetAtmTransactionsStatisticsAsync Tests
    [Fact]
    public async Task GetAtmTransactionsStatisticsAsync_ShouldReturnFailure_WhenException()
    {
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetAllAtmTransactionsAsync(default))
            .ThrowsAsync(new Exception());

        var result = await _bankReportService.GetAtmTransactionsStatisticsAsync();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetAtmTransactionsStatisticsAsync_ShouldReturnSuccess_WhenValid()
    {
        _unitOfWorkMock.Setup(x => x.BankReportRepository.GetAllAtmTransactionsAsync(default))
            .ReturnsAsync(new List<AtmTransaction>());

        var result = await _bankReportService.GetAtmTransactionsStatisticsAsync();
        Assert.True(result.IsSuccess);
    }

    #endregion
}
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExchangeRateApi> _exchangeRateApi;
    private readonly IReportService _reportService;
    public ReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _exchangeRateApi = new Mock<IExchangeRateApi>();

        _reportService = new ReportService(
            _unitOfWorkMock.Object,
            _exchangeRateApi.Object
        );
    }

    [Theory]
    [InlineData("invalidParam")]
    public async Task GetRegisteredUsersCount_ShouldReturnFailure_WhenParamsInvalid(string? param)
    {
        var result = await _reportService.GetRegisteredUsersCountAsync(param);
        Assert.False(result.IsSuccess);
    }


    [Theory]
    [InlineData("current-year")]
    public async Task GetRegisteredUsersCount_ShouldReturnSuccess_WhenParamsAreValid(string? param)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetNumberOfRegisteredUsersAsync(param)).ReturnsAsync(1);
        var result = await _reportService.GetRegisteredUsersCountAsync(param);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("invalidParam")]
    public async Task GetTransactionsCount_ShouldReturnFailure_WhenParamsInvalid(string? param)
    {
        var result = await _reportService.GetTransactionsCountAsync(param);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("last-year")]
    public async Task GetTransactionsCount_ShouldReturnSuccess_WhenParamsAreValid(string? param)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetNumberOfTransactionsAsync(param)).ReturnsAsync(1);
        var result = await _reportService.GetTransactionsCountAsync(param);
        Assert.True(result.IsSuccess);
    }


    [Theory]
    [InlineData("invalidParam", "invalidParam")]
    public async Task GetTransactionsIncomeSum_ShouldReturnFailure_WhenParamsInvalid(string? param, string currency)
    {
        var result = await _reportService.GetTransactionsIncomeSumAsync(param, currency);
        Assert.False(result.IsSuccess);
    }


    [Theory]
    [InlineData("invalidParam", "invalidParam")]
    public async Task GetTransactionsIncomeSum_ShouldReturnFailure_WhenIncomeIsNull(string? param, string currency)
    {
        //_unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsIncomeByCurrencyLastYearAsync(currency)).ReturnsAsync((decimal?)null);
        var result = await _reportService.GetTransactionsIncomeSumAsync(param, currency);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("last-year", "usd")]
    public async Task GetTransactionsIncomeSum_ShouldReturnSuccess_WhenParamsAreValid(string? param, string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsIncomeByCurrencyAsync(param, currency)).ReturnsAsync(1);
        var result = await _reportService.GetTransactionsIncomeSumAsync(param, currency);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("invalidParam")]
    public async Task GetAverageTransactionsIncome_ShouldReturnFailure_WhenAmountIsNull(string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetAverageTransactionsIncomeByCurrencyAsync(currency))
                       .ReturnsAsync((decimal?)null);
        var result = await _reportService.GetAverageTransactionsIncomeAsync(currency);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("usd")]
    public async Task GetAverageTransactionsIncome_ShouldReturnSuccess_WhenAmountIsNotNull(string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetAverageTransactionsIncomeByCurrencyAsync(currency)).ReturnsAsync((decimal?)1);
        var result = await _reportService.GetAverageTransactionsIncomeAsync(currency);
        Assert.True(result.IsSuccess);
    }


    [Fact]
    public async Task GetTransactionsChartForLastMonth_ShouldReturnFailure_WhenTransactionIsNull()
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsChartForLastMonthAsync()).ReturnsAsync(new List<DailyTransaction>(){});
        var result = await _reportService.GetTransactionsChartForLastMonthAsync();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetTransactionsChartForLastMonth_ShouldReturnSuccess_WhenTransactionIsNotNull()
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsChartForLastMonthAsync()).ReturnsAsync(new List<DailyTransaction>(){new()});
        var result = await _reportService.GetTransactionsChartForLastMonthAsync();
        Assert.True(result.IsSuccess);
    }
}

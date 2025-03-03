using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class BankReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IBankReportService _bankReportService;
    public BankReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _bankReportService = new BankReportService(
            _unitOfWorkMock.Object
        );
    }

    [Theory]
    [InlineData("invalidParam", "invalidParam")]
    public async Task GetRegisteredUsersCount_ShouldReturnFailure_WhenParamsInvalid(string? year, string? month)
    {
        var result = await _bankReportService.GetRegisteredUsersCountAsync(year, month);
        Assert.False(result.IsSuccess);
    }


    [Theory]
    [InlineData("current", "previous")]
    public async Task GetRegisteredUsersCount_ShouldReturnSuccess_WhenParamsAreValid(string? year, string? month)
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetNumberOfRegisteredUsersThisYearAsync()).ReturnsAsync(1);
        var result = await _bankReportService.GetRegisteredUsersCountAsync(year, month);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("invalidParam", "invalidParam")]
    public async Task GetTransactionsCount_ShouldReturnFailure_WhenParamsInvalid(string? year, string? month)
    {
        var result = await _bankReportService.GetTransactionsCountAsync(year, month);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("current", "previous")]
    public async Task GetTransactionsCount_ShouldReturnSuccess_WhenParamsAreValid(string? year, string? month)
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetNumberOfTransactionsForLastYearAsync()).ReturnsAsync(1);
        var result = await _bankReportService.GetTransactionsCountAsync(year, month);
        Assert.True(result.IsSuccess);
    }


    [Theory]
    [InlineData("invalidParam", "invalidParam", "invalidParam")]
    public async Task GetTransactionsIncomeSum_ShouldReturnFailure_WhenParamsInvalid(string? year, string? month, string currency)
    {
        var result = await _bankReportService.GetTransactionsIncomeSumAsync(year, month, currency);
        Assert.False(result.IsSuccess);
    }


    [Theory]
    [InlineData("invalidParam", "invalidParam", "invalidParam")]
    public async Task GetTransactionsIncomeSum_ShouldReturnFailure_WhenIncomeIsNull(string? year, string? month, string currency)
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetTransactionsIncomeByCurrencyLastYearAsync(currency)).ReturnsAsync((decimal?)null);
        var result = await _bankReportService.GetTransactionsIncomeSumAsync(year, month, currency);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("previous", "previous", "usd")]
    public async Task GetTransactionsIncomeSum_ShouldReturnSuccess_WhenParamsAreValid(string? year, string? month, string currency)
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetTransactionsIncomeByCurrencyLastYearAsync(currency)).ReturnsAsync(1);
        var result = await _bankReportService.GetTransactionsIncomeSumAsync(year, month, currency);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("invalidParam")]
    public async Task GetAverageTransactionsIncome_ShouldReturnFailure_WhenAmountIsNull(string currency)
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetAverageTransactionsIncomeByCurrencyAsync(currency))
                       .ReturnsAsync((decimal?)null);
        var result = await _bankReportService.GetAverageTransactionsIncomeAsync(currency);
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("usd")]
    public async Task GetAverageTransactionsIncome_ShouldReturnSuccess_WhenAmountIsNotNull(string currency)
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetAverageTransactionsIncomeByCurrencyAsync(currency)).ReturnsAsync((decimal?)1);
        var result = await _bankReportService.GetAverageTransactionsIncomeAsync(currency);
        Assert.True(result.IsSuccess);
    }


    [Fact]
    public async Task GetTransactionsChartForLastMonth_ShouldReturnFailure_WhenTransactionIsNull()
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetTransactionsChartForLastMonthAsync()).ReturnsAsync((IEnumerable<DailyTransaction>)null);
        var result = await _bankReportService.GetTransactionsChartForLastMonthAsync();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetTransactionsChartForLastMonth_ShouldReturnSuccess_WhenTransactionIsNotNull()
    {
        _unitOfWorkMock.Setup(u => u.BankReportRepository.GetTransactionsChartForLastMonthAsync()).ReturnsAsync(new List<DailyTransaction>(){new()});
        var result = await _bankReportService.GetTransactionsChartForLastMonthAsync();
        Assert.True(result.IsSuccess);
    }
}

using System.Collections;
using BankingSystem.Core.DTO.Report;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests;

public class ReportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IReportService _reportService;
    public ReportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _reportService = new ReportService(
            _unitOfWorkMock.Object
        );
    }

    [Theory]
    [InlineData("invalidParam", "invalidParam")]
    public async Task GetRegisteredUsersCount_ShouldReturnFailure_WhenParamsInvalid(string? year, string? month)
    {
        var result = await _reportService.GetRegisteredUsersCountAsync(year, month);
        Assert.False(result.Success);
    }


    [Theory]
    [InlineData("current", "previous")]
    public async Task GetRegisteredUsersCount_ShouldReturnSuccess_WhenParamsAreValid(string? year, string? month)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetNumberOfRegisteredUsersThisYear()).ReturnsAsync(1);
        var result = await _reportService.GetRegisteredUsersCountAsync(year, month);
        Assert.True(result.Success);
    }

    [Theory]
    [InlineData("invalidParam", "invalidParam")]
    public async Task GetTransactionsCount_ShouldReturnFailure_WhenParamsInvalid(string? year, string? month)
    {
        var result = await _reportService.GetTransactionsCountAsync(year, month);
        Assert.False(result.Success);
    }

    [Theory]
    [InlineData("current", "previous")]
    public async Task GetTransactionsCount_ShouldReturnSuccess_WhenParamsAreValid(string? year, string? month)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetNumberOfTransactionsForLastYear()).ReturnsAsync(1);
        var result = await _reportService.GetTransactionsCountAsync(year, month);
        Assert.True(result.Success);
    }


    [Theory]
    [InlineData("invalidParam", "invalidParam", "invalidParam")]
    public async Task GetTransactionsIncomeSum_ShouldReturnFailure_WhenParamsInvalid(string? year, string? month, string currency)
    {
        var result = await _reportService.GetTransactionsIncomeSumAsync(year, month, currency);
        Assert.False(result.Success);
    }


    [Theory]
    [InlineData("invalidParam", "invalidParam", "invalidParam")]
    public async Task GetTransactionsIncomeSum_ShouldReturnFailure_WhenIncomeIsNull(string? year, string? month, string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsIncomeByCurrencyLastYear(currency)).ReturnsAsync((decimal?)null);
        var result = await _reportService.GetTransactionsIncomeSumAsync(year, month, currency);
        Assert.False(result.Success);
    }

    [Theory]
    [InlineData("previous", "previous", "usd")]
    public async Task GetTransactionsIncomeSum_ShouldReturnSuccess_WhenParamsAreValid(string? year, string? month, string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsIncomeByCurrencyLastYear(currency)).ReturnsAsync(1);
        var result = await _reportService.GetTransactionsIncomeSumAsync(year, month, currency);
        Assert.True(result.Success);
    }

    [Theory]
    [InlineData("invalidParam")]
    public async Task GetAverageTransactionsIncome_ShouldReturnFailure_WhenAmountIsNull(string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetAverageTransactionsIncomeByCurrency(currency))
                       .ReturnsAsync((decimal?)null);
        var result = await _reportService.GetAverageTransactionsIncomeAsync(currency);
        Assert.False(result.Success);
    }

    [Theory]
    [InlineData("usd")]
    public async Task GetAverageTransactionsIncome_ShouldReturnSuccess_WhenAmountIsNotNull(string currency)
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetAverageTransactionsIncomeByCurrency(currency)).ReturnsAsync((decimal?)1);
        var result = await _reportService.GetAverageTransactionsIncomeAsync(currency);
        Assert.True(result.Success);
    }


    [Fact]
    public async Task GetTransactionsChartForLastMonth_ShouldReturnFailure_WhenTransactionIsNull()
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsChartForLastMonth()).ReturnsAsync((IEnumerable<DailyTransactionDto>)null);
        var result = await _reportService.GetTransactionsChartForLastMonthAsync();
        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetTransactionsChartForLastMonth_ShouldReturnSuccess_WhenTransactionIsNotNull()
    {
        _unitOfWorkMock.Setup(u => u.ReportRepository.GetTransactionsChartForLastMonth()).ReturnsAsync(new List<DailyTransactionDto>(){new DailyTransactionDto()});
        var result = await _reportService.GetTransactionsChartForLastMonthAsync();
        Assert.True(result.Success);
    }
}

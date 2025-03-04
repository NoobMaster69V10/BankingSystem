using BankingSystem.Core.Extensions;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

public class ReportController(IBankReportService bankReportService) : ControllerBase
{
    [Authorize(Roles = "Manager")]
    [HttpGet("users-count")]

    public async Task<ActionResult<UserStatistics>> GetUserStatistics()
    {
        var result = await bankReportService.GetUserStatisticsAsync();
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }
    [Authorize(Roles = "Manager")]
    [HttpGet("manager-dashboard")]
    public async Task<ActionResult<BankManagerReport>> GetManagerDashboard()
    {
        var result = await bankReportService.GetBankManagerReportAsync();
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { RegisteredCount = result.Value});
    }
    
    [Authorize(Roles = "Manager")]
    [HttpGet("transactions-count")]
    public async Task<ActionResult<TransactionStatistics>> GetTransactionsCount()
    {
        var result = await bankReportService.GetTransactionStatisticsAsync();
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsCount = result.Value });
    }
    
    [Authorize(Roles = "Manager")]
    [HttpGet("transactions-chart")]
    public async Task<ActionResult<IEnumerable<DailyTransactionReport>>> GetTransactionsChartForLastMonth([FromQuery]int days = 30)
    {
        var result = await bankReportService.GetDailyTransactionsAsync(days);
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
    
    [Authorize(Roles = "Manager")]
    [HttpGet("atmtransactions")]
    public async Task<ActionResult<AtmTransactionsStatistics>> GetAtmTransactionsChart()
    {
        var result = await bankReportService.GetAtmTransactionsStatisticsAsync();
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
}

using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

public class ReportController(IReportService reportService) : ControllerBase
{
    [Authorize(Roles = "Manager")]
    [HttpGet("registered-count")]
    public async Task<IActionResult> GetRegisteredUsersCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var result = await reportService.GetRegisteredUsersCountAsync(year, month);

        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { RegisteredCount = result.Value});
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("transactions-count")]
    public async Task<IActionResult> GetTransactionsCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var result = await reportService.GetTransactionsCountAsync(year, month);

        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsCount = result.Value });
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("transactions-income")]
    public async Task<IActionResult> GetTransactionsIncomeSum([FromQuery] string? year, [FromQuery] string? month, [FromQuery] string currency)
    {
        var result = await reportService.GetTransactionsIncomeSumAsync(year, month, currency);

        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsIncome = result.Value });
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("transactions-average-income")]
    public async Task<IActionResult> GetAverageTransactionsIncomeSum([FromQuery] string currency)
    {
        var result = await reportService.GetAverageTransactionsIncomeAsync(currency);

        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsAverageIncome = result.Value });
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("transactions-chart")]
    public async Task<IActionResult> GetTransactionsChartForLastMonth()
    {
        var result = await reportService.GetTransactionsChartForLastMonthAsync();

        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
}

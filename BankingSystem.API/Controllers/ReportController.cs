using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

public class ReportController(IReportService reportService) : ControllerBase
{
    [HttpGet("registered-count")]
    public async Task<IActionResult> GetRegisteredUsersCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var result = await reportService.GetRegisteredUsersCountAsync(year, month);

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpGet("transactions-count")]
    public async Task<IActionResult> GetTransactionsCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var result = await reportService.GetTransactionsCountAsync(year, month);

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }
          
        return Ok(result.Value);
    }

    [HttpGet("transactions-income")]
    public async Task<IActionResult> GetTransactionsIncomeSum([FromQuery] string? year, [FromQuery] string? month, [FromQuery] string currency)
    {
        var result = await reportService.GetTransactionsIncomeSumAsync(year, month, currency);

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpGet("transactions-average-income")]
    public async Task<IActionResult> GetAverageTransactionsIncomeSum([FromQuery] string currency)
    {
        var result = await reportService.GetAverageTransactionsIncomeAsync(currency);

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpGet("transactions-chart")]
    public async Task<IActionResult> GetTransactionsChartForLastMonth()
    {
        var result = await reportService.GetTransactionsChartForLastMonthAsync();

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }

        return Ok(result.Value);
    }
}

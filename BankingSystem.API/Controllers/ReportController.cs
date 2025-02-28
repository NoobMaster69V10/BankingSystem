using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

public class ReportController(IReportService reportService) : ControllerBase
{
    [HttpGet("registered-count")]
    public async Task<IActionResult> GetRegisteredUsersCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var response = await reportService.GetRegisteredUsersCountAsync(year, month);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(new { Count = response.Data });
    }

    [HttpGet("transactions-count")]
    public async Task<IActionResult> GetTransactionsCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var response = await reportService.GetTransactionsCountAsync(year, month);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }
          
        return Ok(new { Count = response.Data });
    }

    [HttpGet("transactions-income")]
    public async Task<IActionResult> GetTransactionsIncomeSum([FromQuery] string? year, [FromQuery] string? month, [FromQuery] string currency)
    {
        var response = await reportService.GetTransactionsIncomeSumAsync(year, month, currency);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(new { Income = response.Data });
    }

    [HttpGet("transactions-average-income")]
    public async Task<IActionResult> GetAverageTransactionsIncomeSum([FromQuery] string currency)
    {
        var response = await reportService.GetAverageTransactionsIncomeAsync(currency);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(new { AverageIncome = response.Data });
    }

    [HttpGet("transactions-chart")]
    public async Task<IActionResult> GetTransactionsChartForLastMonth()
    {
        var response = await reportService.GetTransactionsChartForLastMonthAsync();

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(response.Data);
    }
}

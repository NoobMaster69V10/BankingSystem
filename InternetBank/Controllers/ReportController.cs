using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;

public class ReportController(IReportService reportService) : CustomControllerBase
{
    [HttpGet("registered-count")]
    public async Task<IActionResult> GetRegisteredUsersCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var response = await reportService.GetRegisteredUsersCount(year, month);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(new { Count = response.Data });
    }

    [HttpGet("transactions-count")]
    public async Task<IActionResult> GetTransactionsCount([FromQuery] string? year, [FromQuery] string? month)
    {
        var response = await reportService.GetTransactionsCount(year, month);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }
          
        return Ok(new { Count = response.Data });
    }

    [HttpGet("transactions-income")]
    public async Task<IActionResult> GetTransactionsIncomeSum([FromQuery] string? year, [FromQuery] string? month, [FromQuery] string currency)
    {
        var response = await reportService.GetTransactionsIncomeSum(year, month, currency);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(new { Income = response.Data });
    }

    [HttpGet("transactions-average-income")]
    public async Task<IActionResult> GetAverageTransactionsIncomeSum([FromQuery] string currency)
    {
        var response = await reportService.GetAverageTransactionsIncome(currency);

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(new { AverageIncome = response.Data });
    }

    [HttpGet("transactions-chart")]
    public async Task<IActionResult> GetTransactionsChartForLastMonth()
    {
        var response = await reportService.GetTransactionsChartForLastMonth();

        if (!response.Success)
        {
            return BadRequest(new { ErrorMessage = response.Message });
        }

        return Ok(response.Data);
    }
}

using BankingSystem.Core.Extensions;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

/// <summary>
/// Provides banking system reports and statistics for managers.
/// </summary>
/// <remarks>
/// This controller offers various reports on system usage, user statistics, and transaction data.
/// Access is restricted to users with the Manager role.
/// </remarks>
[ApiController]
[Authorize(Roles = "Manager")]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IBankReportService _bankReportService;

    public ReportController(IBankReportService bankReportService)
    {
        _bankReportService = bankReportService;
    }
    /// <summary>
    /// Retrieves statistics about user accounts in the system.
    /// </summary>
    /// <returns>User statistics including registration counts and account types.</returns>
    /// <response code="200">Returns the user statistics data.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Manager.</response>
    [HttpGet("users-count")]
    [ProducesResponseType(typeof(UserStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserStatistics>> GetUserStatistics()
    {
        var result = await _bankReportService.GetUserStatisticsAsync();
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }

    /// <summary>
    /// Retrieves the bank manager dashboard summary data.
    /// </summary>
    /// <returns>Dashboard summary for bank managers.</returns>
    /// <response code="200">Returns the manager dashboard data.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Manager.</response>
    [HttpGet("manager-dashboard")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BankManagerReport>> GetManagerDashboard()
    {
        var result = await _bankReportService.GetBankManagerReportAsync();
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { RegisteredCount = result.Value});
    }
    
    /// <summary>
    /// Retrieves statistics about banking transactions in the system.
    /// </summary>
    /// <returns>Transaction count statistics grouped by type.</returns>
    /// <response code="200">Returns the transaction statistics.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Manager.</response>
    [HttpGet("transactions-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TransactionStatistics>> GetTransactionsCount()
    {
        var result = await _bankReportService.GetTransactionStatisticsAsync();
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsCount = result.Value });
    }
    
    /// <summary>
    /// Retrieves daily transaction data for charting over the specified period.
    /// </summary>
    /// <param name="days">The number of past days to include in the report (default: 30).</param>
    /// <returns>A collection of daily transaction reports.</returns>
    /// <response code="200">Returns the daily transaction chart data.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Manager.</response>
    [HttpGet("transactions-chart")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DailyTransactionReport>>> GetTransactionsChartForLastMonth([FromQuery]int days = 30)
    {
        var result = await _bankReportService.GetDailyTransactionsAsync(days);
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
    
    /// <summary>
    /// Retrieves statistics about ATM transactions in the system.
    /// </summary>
    /// <returns>ATM transaction statistics including withdrawals, deposits, and other operations.</returns>
    /// <response code="200">Returns the ATM transaction statistics.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Manager.</response>
    [HttpGet("atm-transactions")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AtmTransactionsStatistics>> GetAtmTransactionsChart()
    {
        var result = await _bankReportService.GetAtmTransactionsStatisticsAsync();
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
}
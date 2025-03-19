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
    [HttpGet("users-count")]
    [ProducesResponseType(typeof(UserStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserStatistics>> GetUserStatistics(CancellationToken cancellationToken)
    {
        var result = await _bankReportService.GetUserStatisticsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }

    /// <summary>
    /// Retrieves the bank manager dashboard summary data.
    /// </summary>
    /// <returns>Dashboard summary for bank managers.</returns>
    /// <response code="200">Returns the manager dashboard data.</response>
    [HttpGet("manager-dashboard")]
    [ProducesResponseType(typeof(BankManagerReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<BankManagerReport>> GetManagerDashboard(CancellationToken cancellationToken)
    {
        var result = await _bankReportService.GetBankManagerReportAsync(cancellationToken);
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { RegisteredCount = result.Value});
    }
    
    /// <summary>
    /// Retrieves statistics about banking transactions in the system.
    /// </summary>
    /// <returns>Transaction count statistics grouped by type.</returns>
    /// <response code="200">Returns the transaction statistics.</response>
    [HttpGet("transactions-count")]
    [ProducesResponseType(typeof(TransactionStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionStatistics>> GetTransactionsCount(CancellationToken cancellationToken)
    {
        var result = await _bankReportService.GetTransactionStatisticsAsync(cancellationToken);
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsCount = result.Value });
    }

    /// <summary>
    /// Retrieves daily transaction data for charting over the specified period.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="days">The number of past days to include in the report (default: 30).</param>
    /// <returns>A collection of daily transaction reports.</returns>
    /// <response code="200">Returns the daily transaction chart data.</response>
    [HttpGet("transactions-chart")]
    [ProducesResponseType(typeof(IEnumerable<DailyTransactionReport>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DailyTransactionReport>>> GetTransactionsChartForLastMonth(CancellationToken cancellationToken, [FromQuery]int days = 30)
    {
        var result = await _bankReportService.GetDailyTransactionsAsync(days, cancellationToken);
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
    
    /// <summary>
    /// Retrieves statistics about ATM transactions in the system.
    /// </summary>
    /// <returns>ATM transaction statistics including withdrawals, deposits, and other operations.</returns>
    /// <response code="200">Returns the ATM transaction statistics.</response>
    [HttpGet("atm-transactions")]
    [ProducesResponseType(typeof(AtmTransactionsStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<AtmTransactionsStatistics>> GetAtmTransactionsChart(CancellationToken cancellationToken)
    {
        var result = await _bankReportService.GetAtmTransactionsStatisticsAsync(cancellationToken);
        return !result.IsSuccess ? result.ToProblemDetails() : Ok(new { TransactionsChart = result.Value });
    }
}
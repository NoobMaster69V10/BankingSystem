using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.Response;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

/// <summary>
/// Provides ATM functionality including balance inquiries, withdrawals, deposits, and PIN management.
/// </summary>
/// <remarks>
/// This controller simulates ATM operations that customers can perform with their bank cards.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AtmController : ControllerBase
{
    private readonly IAtmService _atmService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtmController"/> class.
    /// </summary>
    /// <param name="atmService">The service for handling ATM operations.</param>
    public AtmController(IAtmService atmService)
    {
        _atmService = atmService;
    }
    
    /// <summary>
    /// Displays the current balance for a bank account associated with a bank card.
    /// </summary>
    /// <param name="cardDto">The card authorization details including card number and PIN.</param>
    /// <returns>The current balance information.</returns>
    /// <response code="200">Returns the account balance information.</response>
    /// <response code="400">If the card information is invalid or authorization fails.</response>
    [HttpPost("balance")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BalanceResponse>> ShowBalance(CardAuthorizationDto cardDto)
    {
        var result = await _atmService.ShowBalanceAsync(cardDto);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Withdraws money from a bank account associated with a bank card.
    /// </summary>
    /// <param name="cardDto">The withdrawal details including card number, PIN, and amount.</param>
    /// <returns>The transaction response with updated balance.</returns>
    /// <response code="200">Returns the transaction details and updated balance.</response>
    /// <response code="400">If withdrawal fails due to insufficient funds, invalid PIN, or other errors.</response>
    [HttpPost("withdraw-money")]
    [ProducesResponseType(typeof(AtmTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AtmTransactionResponse>> WithdrawMoney(WithdrawMoneyDto cardDto)
    {
        var result = await _atmService.WithdrawMoneyAsync(cardDto);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Changes the PIN number for a bank card.
    /// </summary>
    /// <param name="cardDto">The PIN change details including card number, current PIN, and new PIN.</param>
    /// <returns>A confirmation message if PIN change is successful.</returns>
    /// <response code="200">Returns confirmation of PIN change.</response>
    /// <response code="400">If PIN change fails due to invalid current PIN or other validation errors.</response>
    [HttpPatch("change-pin")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> ChangePin(ChangePinDto cardDto)
    {
        var result = await _atmService.ChangePinAsync(cardDto);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Deposits money into a bank account associated with a bank card.
    /// </summary>
    /// <param name="cardDto">The deposit details including card number, PIN, and amount.</param>
    /// <returns>The updated balance after deposit.</returns>
    /// <response code="200">Returns the updated balance information.</response>
    /// <response code="400">If deposit fails due to invalid card information or other errors.</response>
    [HttpPost("deposit-money")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BalanceResponse>> DepositMoney(DepositMoneyDto cardDto)
    {
        var result = await _atmService.DepositMoneyAsync(cardDto);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
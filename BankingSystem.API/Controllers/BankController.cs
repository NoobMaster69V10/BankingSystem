using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;

/// <summary>
/// Manages bank accounts, cards, and money transfers.
/// </summary>
/// <remarks>
/// This controller provides endpoints for creating bank accounts and cards,
/// as well as transferring money between accounts.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class BankController : ControllerBase
{
    private readonly IBankAccountService _accountService;
    private readonly IBankCardService _cardService;

    public BankController(IBankAccountService accountService, IBankCardService cardService)
    {
        _accountService = accountService;
        _cardService = cardService;
    }

    /// <summary>
    /// Creates a new bank account for a customer.
    /// </summary>
    /// <param name="bankAccountRegisterDto">The bank account registration details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The newly created bank account information.</returns>
    /// <response code="201">Returns the newly created bank account.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not an Operator.</response>
    [Authorize(Roles = "Operator")]
    [HttpPost("account")]
    [ProducesResponseType(typeof(BankAccount), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BankAccount>> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto, CancellationToken cancellationToken)
    {   
        var result = await _accountService.CreateBankAccountAsync(bankAccountRegisterDto, cancellationToken);
        return result.IsSuccess ? Created("account", result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Creates a new bank card linked to a bank account.
    /// </summary>
    /// <param name="cardRegisterDto">The bank card registration details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The newly created bank card information.</returns>
    /// <response code="201">Returns the newly created bank card.</response>
    /// <response code="400">If the provided data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not an Operator.</response>
    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    [ProducesResponseType(typeof(BankCard), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BankCard>>CreateBankCard(BankCardRegisterDto cardRegisterDto, CancellationToken cancellationToken)
    {
        var result = await _cardService.CreateBankCardAsync(cardRegisterDto, cancellationToken);
        return result.IsSuccess ? Created("card", result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Transfers money between bank accounts.
    /// </summary>
    /// <param name="transactionDto">The transaction details including source account, destination account, and amount.</param>
    /// <param name="accountTransactionService">The service for handling account transactions.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The completed account transfer information.</returns>
    /// <response code="201">Returns the completed transfer details.</response>
    /// <response code="400">If the transfer could not be completed due to insufficient funds or other errors.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not a Person or trying to access another user's account.</response>
    [Authorize(Roles = "Person")]
    [HttpPost("transfer-money")]
    [ProducesResponseType(typeof(AccountTransfer), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AccountTransfer>> TransferMoney(
        AccountTransactionDto transactionDto, 
        [FromServices]IAccountTransactionService accountTransactionService, 
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, userId, cancellationToken);

        return result.IsFailure ? result.ToProblemDetails() : Created("transfer-money", result.Value);
    }
    [HttpDelete("card-delete")]
    [Authorize]
    public async Task<ActionResult<CardRemovalResponse>> RemoveBankCard(string cardNumber,CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await _cardService.RemoveBankCardAsync(cardNumber,userId,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
    [HttpDelete("account-delete")]
    [Authorize]
    public async Task<ActionResult<AccountRemovalResponse>> RemoveBankAccount(string iban,CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await _accountService.RemoveBankAccountAsync(iban,userId,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
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
    /// <param name="cancellationToken"></param>
    /// <returns>The newly created bank account information.</returns>
    /// <response code="201">Returns the newly created bank account.</response>
    [Authorize(Roles= nameof(Role.User))]
    [HttpPost("account")]
    [ProducesResponseType(typeof(BankAccount), StatusCodes.Status201Created)]
    public async Task<ActionResult<BankAccount>> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto, CancellationToken cancellationToken)
    {   
        var result = await _accountService.CreateBankAccountAsync(bankAccountRegisterDto, cancellationToken);
        return result.IsSuccess ? Created("account", result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Creates a new bank card linked to a bank account.
    /// </summary>
    /// <param name="cardRegisterDto">The bank card registration details.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The newly created bank card information.</returns>
    /// <response code="201">Returns the newly created bank card.</response>
    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    [ProducesResponseType(typeof(BankCard), StatusCodes.Status201Created)]
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
    /// <param name="cancellationToken"></param>
    /// <returns>The completed account transfer information.</returns>
    /// <response code="201">Returns the completed transfer details.</response>
    [Authorize(Roles = nameof(Role.User))]
    [HttpPost("transfer-money")]
    [ProducesResponseType(typeof(AccountTransfer), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountTransfer>> TransferMoney(
        AccountTransactionDto transactionDto, 
        [FromServices]IAccountTransactionService accountTransactionService, 
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, userId, cancellationToken);

        return result.IsFailure ? result.ToProblemDetails() : Created("transfer-money", result.Value);
    }

    /// <summary>
    /// Deactivates a bank card to prevent further use.
    /// </summary>
    /// <param name="cardNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Bank card information.</returns>
    /// <response code="200">Returns a message about deactivation.</response>
    [HttpPatch("card-deactivate")]
    [Authorize(Roles = nameof(Role.User))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> DeactivateBankCard(string cardNumber, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await _cardService.DeactivateBankCardAsync(cardNumber, userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Deletes a bank card to prevent further use.
    /// </summary>
    /// <param name="bankCardActiveDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Bank card information.</returns>
    /// <response code="200">Returns a message about bank card.</response>
    [HttpDelete("card-delete")]
    [Authorize(Roles = nameof(Role.Operator))]
    [ProducesResponseType(typeof(CardRemovalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CardRemovalResponse>> RemoveBankCard(BankCardActiveDto bankCardActiveDto,CancellationToken cancellationToken)
    {
        var result = await _cardService.RemoveBankCardAsync(bankCardActiveDto,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }


    /// <summary>
    /// Activates a bank card.
    /// </summary>
    /// <param name="cardNumber">The bank card number.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns activation result.</returns>
    /// <response code="201">Returns activation result.</response>
    [HttpPatch("activate-card")]
    [Authorize(Roles = nameof(Role.User))]
    public async Task<ActionResult<string>> ActivateBankCard(string cardNumber, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await _cardService.ActivateBankCardAsync(cardNumber, userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    /// <summary>
    /// Deletes a bank account to prevent further use.
    /// </summary>
    /// <param name="bankAccountRemovalDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Bank account information.</returns>
    /// <response code="200">Returns a message about bank account.</response>
    [HttpDelete("bank-account")]
    [Authorize(Roles = nameof(Role.Operator))]
    [ProducesResponseType(typeof(AccountRemovalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountRemovalResponse>> RemoveBankAccount(BankAccountRemovalDto bankAccountRemovalDto,CancellationToken cancellationToken)
    { 
        var result = await _accountService.RemoveBankAccountAsync(bankAccountRemovalDto,cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
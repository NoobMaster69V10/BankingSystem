using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BankController(IBankAccountService accountService, IBankCardService cardService) : ControllerBase
{
    [Authorize(Roles = "Operator")]
    [HttpPost("account")]
    public async Task<ActionResult<BankAccount>> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto, CancellationToken cancellationToken)
    {   
        var result = await accountService.CreateBankAccountAsync(bankAccountRegisterDto, cancellationToken);
        return result.IsSuccess ? Created("account", result.Value) : result.ToProblemDetails();
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    public async Task<ActionResult<BankCard>>CreateBankCard(BankCardRegisterDto cardRegisterDto, CancellationToken cancellationToken)
    {
        var result = await cardService.CreateBankCardAsync(cardRegisterDto, cancellationToken);
        return result.IsSuccess ? Created("card", result.Value) : result.ToProblemDetails();
    }

    [Authorize(Roles = "Person")]
    [HttpPost("transfer-money")]
    public async Task<ActionResult<AccountTransfer>> TransferMoney(AccountTransactionDto transactionDto, [FromServices]IAccountTransactionService accountTransactionService, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, userId, cancellationToken);

        return result.IsFailure ? result.ToProblemDetails() : Created("transfer-money", result.Value);
    }

}
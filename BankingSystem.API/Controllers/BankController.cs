using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
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
    public async Task<ActionResult<BankAccount>> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto)
    {   
        var result = await accountService.CreateBankAccountAsync(bankAccountRegisterDto);
        return result.IsSuccess ? Created("account", result.Value) : result.ToProblemDetails();
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    public async Task<ActionResult<BankCard>>CreateBankCard(BankCardRegisterDto cardRegisterDto)
    {
        var result = await cardService.CreateBankCardAsync(cardRegisterDto);
        return result.IsSuccess ? Created("card", result.Value) : result.ToProblemDetails();
    }

    [Authorize(Roles = "Person")]
    [HttpPost("transfer-money")]
    public async Task<ActionResult<AccountTransaction>> TransferMoney(AccountTransactionDto transactionDto, [FromServices]IAccountTransactionService accountTransactionService)
    {
        var userId = User.FindFirst("personId")!.Value;
        var result = await accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

        return result.IsFailure ? result.ToProblemDetails() : Created("transfer-money", result.Value);
    }

}
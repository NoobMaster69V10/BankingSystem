using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BankController(IBankAccountService accountService, IBankCardService cardService) : ControllerBase
{
    [Authorize(Roles = "Operator")]
    [HttpPost("account")]
    public async Task<IActionResult> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto)
    {   
        var result = await accountService.CreateBankAccountAsync(bankAccountRegisterDto);
        return result.IsSuccess ? Created("account", result.Value) : result.ToProblemDetails();
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    public async Task<IActionResult> CreateBankCard(BankCardRegisterDto cardRegisterDto)
    {
        var result = await cardService.CreateBankCardAsync(cardRegisterDto);
        return result.IsSuccess ? Created("card", result.Value) : result.ToProblemDetails();
    }
}
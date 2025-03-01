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
        if(result.IsSuccess)
        {
            return Created("account", result.Value);
        }
        return result.ToProblemDetails();
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    public async Task<IActionResult> CreateBankCard(BankCardRegisterDto cardRegisterDto)
    {
        var result = await cardService.CreateBankCardAsync(cardRegisterDto);
        if (result.IsSuccess)
        {
            return Created("card", result.Value);
        }
        return result.ToProblemDetails();
    }
}
using BankingSystem.Core.DTO;
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
        var response = await accountService.CreateBankAccountAsync(bankAccountRegisterDto);
        if(response.IsSuccess)
        {
            return Created("account", response.Value);
        }
        return BadRequest(response);
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    public async Task<IActionResult> CreateBankCard(BankCardRegisterDto cardRegisterDto)
    {
        var response = await cardService.CreateBankCardAsync(cardRegisterDto);
        if (response.IsSuccess)
        {
            return Created("card", response.Value);
        }
        return BadRequest(response);
    }
}
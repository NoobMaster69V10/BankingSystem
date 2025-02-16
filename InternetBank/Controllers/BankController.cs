using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;

public class BankController(IBankAccountService accountService, IBankCardService cardService) : CustomControllerBase
{
    [Authorize(Roles = "Operator")]
    [HttpPost("account")]
    public async Task<ActionResult<ApiResponse>> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto)
    {
        var response = await accountService.CreateBankAccountAsync(bankAccountRegisterDto);
        if(response.IsSuccess)
        {
            return Ok(response);
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
            return Ok(response);
        }
        return BadRequest(response);
    }
}
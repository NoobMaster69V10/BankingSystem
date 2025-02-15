using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;

public class BankController(IBankAccountService accountService, IBankCardService cardService) : CustomControllerBase
{
    [Authorize(Roles = "Operator")]
    [HttpPost("account")]
    public async Task<IActionResult> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto)
    {
        await accountService.CreateBankAccountAsync(bankAccountRegisterDto);

        return Ok(new { message = "Account created successfully" });
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("card")]
    public async Task<IActionResult> CreateBankCard(BankCardRegisterDto cardRegisterDto)
    {
        await cardService.CreateBankCardAsync(cardRegisterDto);
        return Ok(new { message = "Card created successfully" });
    }
}
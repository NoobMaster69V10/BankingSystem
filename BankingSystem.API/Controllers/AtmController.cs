using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AtmController : ControllerBase
{
    private readonly IAtmService _atmService;
    private readonly IAccountTransactionService _accountTransactionService;

    public AtmController(IAtmService atmService, IAccountTransactionService accountTransactionService)
    {
        _atmService = atmService;
        _accountTransactionService = accountTransactionService;
    }
    
    [HttpPost("balance")]
    public async Task<IActionResult> ShowBalance(CardAuthorizationDto cardDto)
    {
        var result = await _atmService.ShowBalanceAsync(cardDto);
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }

    [HttpPost("withdraw-money")]
    public async Task<IActionResult> WithdrawMoney([FromBody] WithdrawMoneyDto cardDto)
    {
        var result = await _accountTransactionService.WithdrawMoneyAsync(cardDto);
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }

    [HttpPost("change-pin")]
    public async Task<IActionResult> ChangePin(ChangePinDto cardDto)
    {
        var result = await _atmService.ChangePinAsync(cardDto);
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }
}
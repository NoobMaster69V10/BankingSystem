using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;
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
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPost("withdraw-money")]
    public async Task<ActionResult<ApiResponse>> WithdrawMoney([FromBody] WithdrawMoneyDto cardDto)
    {
        var response = await _accountTransactionService.WithdrawMoneyAsync(cardDto);
        return response.IsSuccess ? Ok(response) : BadRequest(response);

    }

    [HttpPost("change-pin")]
    public async Task<ActionResult<ApiResponse>> ChangePin(ChangePinDto cardDto)
    {
        var response = await _atmService.ChangePinAsync(cardDto);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
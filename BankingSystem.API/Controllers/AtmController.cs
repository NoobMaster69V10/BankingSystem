using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AtmController : ControllerBase
{
    private readonly IAtmService _atmService;

    public AtmController(IAtmService atmService)
    {
        _atmService = atmService;
    }
    
    [HttpPost("balance")]
    public async Task<ActionResult<BalanceResponse>> ShowBalance(CardAuthorizationDto cardDto)
    {
        var result = await _atmService.ShowBalanceAsync(cardDto);
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }

    [HttpPost("withdraw-money")]
    public async Task<ActionResult<AtmTransactionResponse>> WithdrawMoney([FromBody] WithdrawMoneyDto cardDto)
    {
        var result = await _atmService.WithdrawMoneyAsync(cardDto);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("change-pin")]
    public async Task<ActionResult<string>> ChangePin(ChangePinDto cardDto)
    {
        var result = await _atmService.ChangePinAsync(cardDto);
        return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
    }
}
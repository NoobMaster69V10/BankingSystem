using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;
public class AtmController : CustomControllerBase
{
    private readonly IAtmService _atmService;
    private readonly IAccountTransactionService _accountTransactionService;

    public AtmController(IAtmService atmService, IAccountTransactionService accountTransactionService)
    {
        _atmService = atmService;
        _accountTransactionService = accountTransactionService;
    }

    [HttpPost("authorize-card")]
    public async Task<ActionResult<ApiResponse>> Authorize(CardAuthorizationDto cardDto)
    {
        var response = await _atmService.AuthorizeCardAsync(cardDto);
        if (response.IsSuccess)
        {
            HttpContext.Session.SetString("AuthorizedCard", cardDto.CardNumber);
            HttpContext.Session.SetString("LastLogin", DateTime.Now.ToString());
            HttpContext.Session.SetInt32("LoginAttempts", 0);
            
            return Ok(response);
        }
        return BadRequest(response);
    }
     
   
    [HttpPost("balance")]
    public async Task<ActionResult<ApiResponse>> ShowBalance()
    {
        var cardNumber = HttpContext.Session.GetString("AuthorizedCard");
        if (string.IsNullOrEmpty(cardNumber))
        {
            return Unauthorized(new ApiResponse 
            { 
                IsSuccess = false, 
                ErrorMessages = ["Please authorize your card first"]
            });
        }
        var response = await _atmService.ShowBalanceAsync(cardNumber);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
    [HttpPost("change-pin")]
    public async Task<ActionResult<ApiResponse>> ChangePin([FromBody]ChangePinDto pinDto)
    {
        var cardNumber = HttpContext.Session.GetString("AuthorizedCard");
        if (string.IsNullOrEmpty(cardNumber))
        {
            return Unauthorized(new ApiResponse
            {
                IsSuccess = false,
                ErrorMessages = ["Please authorize your card first"]
            });
        }
        pinDto.CardNumber = cardNumber;
        var response = await _atmService.ChangePinAsync(pinDto);
        
        if (response.IsSuccess)
        {
            HttpContext.Session.Clear();  
            return Ok(response);
        }

        return BadRequest(response);

    }
    
    [HttpPost("withdraw-money")]
    public async Task<ActionResult<ApiResponse>> WithdrawMoney([FromBody]WithdrawMoneyDto withdrawMoneyDto)
    {
        var cardNumber = HttpContext.Session.GetString("AuthorizedCard");
        if (string.IsNullOrEmpty(cardNumber))
        {
            return Unauthorized(new ApiResponse
            {
                IsSuccess = false,
                ErrorMessages = ["Please authorize your card first"]
            });
        }
        withdrawMoneyDto.CardNumber = cardNumber;
        var response = await _accountTransactionService.WithdrawMoneyAsync(withdrawMoneyDto);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
    
    
    [HttpPost("logout")]
    public ActionResult<ApiResponse> Logout()
    {
        HttpContext.Session.Clear();
        
        return Ok(new ApiResponse 
        { 
            IsSuccess = true, 
        });
    }
    // [HttpPost("authorize-card")]
    // public async Task<ActionResult<ApiResponse>> Authorize(CardAuthorizationDto cardAuthorizationDto)
    // {
    //     var response = await _atmService.AuthorizeCardAsync(cardAuthorizationDto);
    //     if (response.IsSuccess)
    //     {
    //         return Ok(response);
    //     }
    //     return BadRequest(response);
    // }
    
    
    // [HttpPost("balance")]
    // public async Task<ActionResult<ApiResponse>> ShowBalance(CardAuthorizationDto cardDto)
    // {
    //     var response = await _atmService.ShowBalanceAsync(cardDto);
    //     if (response.IsSuccess)
    //     {
    //         return Ok(response);
    //     }
    //     return BadRequest(response);
    // }

}
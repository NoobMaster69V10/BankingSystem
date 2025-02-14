using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OperatorController : ControllerBase
{
    private readonly IPersonAuthService _authService;
    private readonly IBankAccountService _accountService;
    private readonly IBankCardService _cardService;


    public OperatorController(IPersonAuthService authService, IBankAccountService accountService, IBankCardService cardService)
    {
        _authService = authService;
        _accountService = accountService;
        _cardService = cardService;
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] PersonRegisterDto registerModel)
    {
        if (!await _authService.RegisterPersonAsync(registerModel))
        {
            return BadRequest("Invalid operation.");
        }
        return Ok(new { message = "User registered successfully!" });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] PersonLoginDto loginModel)
    {
        var result = await _authService.AuthenticationPersonAsync(loginModel);

        if (result == null || string.IsNullOrEmpty(result.Token))
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(result);
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("create-bank-account")]
    public async Task<IActionResult> CreateBankAccount(BankAccountRegisterDto bankAccountRegisterDto)
    {
        await _accountService.CreateBankAccountAsync(bankAccountRegisterDto);

        return Ok(new { message = "Account created successfully" });
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("create-bank-card")]
    public async Task<IActionResult> CreateBankCard(BankCardRegisterDto cardRegisterDto)
    {
        
        await _cardService.CreateBankCardAsync(cardRegisterDto);
        return Ok(new { message = "Card created successfully" });
    }
}
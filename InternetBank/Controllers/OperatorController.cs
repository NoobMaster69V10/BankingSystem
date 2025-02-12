using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OperatorController : ControllerBase
{
    private readonly UserManager<IdentityPerson> _userManager;
    private readonly IAuthService _authService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IBankAccountService _accountService;
    private readonly IBankCardService _cardService;


    public OperatorController(UserManager<IdentityPerson> userManager, IAuthService authService, RoleManager<IdentityRole> roleManager, IBankAccountService accountService, IBankCardService cardService)
    {
        _userManager = userManager;
        _authService = authService;
        _roleManager = roleManager;
        _accountService = accountService;
        _cardService = cardService;
    }

    [Authorize(Roles = "Operator")]
    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] PersonRegisterDto registerModel)
    {
        var user = new IdentityPerson
        {
            UserName = registerModel.Email,
            Email = registerModel.Email,
            Name = registerModel.Name,
            Lastname = registerModel.Lastname,
            BirthDate = registerModel.BirthDate,
            IdNumber = registerModel.IdNumber
        };

        var result = await _userManager.CreateAsync(user, registerModel.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (string.IsNullOrEmpty(registerModel.Role))
            registerModel.Role = "User";

        if (!await _roleManager.RoleExistsAsync(registerModel.Role))
            return BadRequest("Invalid role specified.");

        await _userManager.AddToRoleAsync(user, registerModel.Role);

        return Ok(new { message = "User registered successfully!" });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] PersonLoginDto loginModel)
    {
        var user = await _userManager.FindByEmailAsync(loginModel.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new { token.Result });
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
        //var userId = User.FindFirst("userId")!.Value;
        await _cardService.CreateBankCardAsync(cardRegisterDto);
        return Ok(new { message = "Card created successfully" });
    }
}
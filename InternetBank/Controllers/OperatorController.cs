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
    private readonly UserManager<User> _userManager;
    private readonly IAuthService _authService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAccountService _accountService;
    private readonly ICardService _cardService;


    public OperatorController(UserManager<User> userManager, IAuthService authService, RoleManager<IdentityRole> roleManager, IAccountService accountService, ICardService cardService)
    {
        _userManager = userManager;
        _authService = authService;
        _roleManager = roleManager;
        _accountService = accountService;
        _cardService = cardService;
    }

    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] CustomerRegisterDto registerModel)
    {
        var user = new User
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
    public async Task<IActionResult> Login([FromBody] CustomerLoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new { token.Result });
    }

    [Authorize]
    [HttpPost("create-account")]
    public async Task<IActionResult> CreateAccount(BankAccountRegisterDto bankAccountRegisterDto)
    {
        var userId = User.FindFirst("userId")!.Value;
        await _accountService.CreateAccountAsync(bankAccountRegisterDto, userId);

        return Ok(new { message = "Account created successfully" });
    }

    [Authorize]
    [HttpPost("create-card")]
    public async Task<IActionResult> CreateCard(BankCardRegisterDto cardRegisterDto)
    {
        var userId = User.FindFirst("userId")!.Value;

        await _cardService.CreateCardAsync(cardRegisterDto, userId);
        return Ok(new { message = "Card created successfully" });
    }
}
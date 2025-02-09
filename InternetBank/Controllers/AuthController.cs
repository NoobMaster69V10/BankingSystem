using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthService _authService;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(UserManager<User> userManager, IAuthService authService, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _authService = authService;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterDto registerModel)
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
}
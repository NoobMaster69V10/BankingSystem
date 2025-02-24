using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(
        IPersonAuthService personAuthService,
        IAccountTransactionService transactionService,
        IPersonService personService,
        IEmailService emailService) : ControllerBase
    {
        [Authorize(Roles = "Person")]
        [HttpPost("transfer-money")]
        public async Task<IActionResult> TransferMoney(TransactionDto transactionDto)
        {
            var userId = User.FindFirst("personId")!.Value;
            var response = await transactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            if (response.IsFailure)
            {
                return BadRequest(response.Error);
            }
            return Created("transfer-money", response.Value);
        }

        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<IActionResult> GetPersonInfo()
        {
            var personId = User.FindFirst("personId")!.Value;

            var response = await personService.GetPersonById(personId);
            if (response.IsFailure)
            {
                return BadRequest(response);
            }

            return Ok(response.Value);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] PersonRegisterDto registerModel)
        {
            var response = await personAuthService.RegisterPersonAsync(registerModel);
            if (response.IsFailure)
            {
                return BadRequest(response);
            }

            return Created("register", response.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PersonLoginDto loginModel)
        {
            var response = await personAuthService.AuthenticationPersonAsync(loginModel);
            if (response.IsFailure)
            {
                return BadRequest(response);
            }
            return Created("login", new { Token = response.Value });
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            var result = await personAuthService.ForgotPasswordAsync(forgotPassword);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            var result = await personAuthService.ResetPasswordAsync(resetPassword);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }
    }
}
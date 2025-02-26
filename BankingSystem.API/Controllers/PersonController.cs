using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sprache;

namespace BankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(
        IPersonAuthService personAuthService,
        IAccountTransactionService transactionService,
        IPersonService personService) : ControllerBase
    {
        [Authorize(Roles = "Person")]
        [HttpPost("transfer-money")]
        public async Task<IActionResult> TransferMoney(TransactionDto transactionDto)
        {
            var userId = User.FindFirst("personId")!.Value;
            var result = await transactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            return Created("transfer-money", result.Value);
        }

        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<IActionResult> GetPersonInfo()
        {
            var personId = User.FindFirst("personId")!.Value;

            var result = await personService.GetPersonById(personId);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return Ok(result.Value);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] PersonRegisterDto registerModel)
        {
            var result = await personAuthService.RegisterPersonAsync(registerModel);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return Created("register", result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PersonLoginDto loginModel)
        {
            var result = await personAuthService.AuthenticationPersonAsync(loginModel);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            return Created("login", new { Token = result.Value });
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            var result = await personAuthService.ForgotPasswordAsync(forgotPassword);
            return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            var result = await personAuthService.ResetPasswordAsync(resetPassword);
            return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
        }
    }
}
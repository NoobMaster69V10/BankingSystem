using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(
        IPersonAuthService personAuthService,
        IAccountTransactionService accountTransactionService,
        IPersonService personService) : ControllerBase
    {
        [Authorize(Roles = "Person")]
        [HttpPost("transfer-money")]
        public async Task<ActionResult<AccountTransaction>> TransferMoney(AccountTransactionDto transactionDto)
        {
            var userId = User.FindFirst("personId")!.Value;
            var result = await accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            return result.IsFailure ? result.ToProblemDetails() : Created("transfer-money", result.Value);
        }

        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<ActionResult<Person>> GetPersonInfo()
        {
            var personId = User.FindFirst("personId")!.Value;

            var result = await personService.GetPersonById(personId);
            return result.IsFailure ? result.ToProblemDetails() : Ok(result.Value);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> RegisterUser([FromBody] PersonRegisterDto registerModel)
        {
            var result = await personAuthService.RegisterPersonAsync(registerModel);
            return result.IsFailure ? result.ToProblemDetails() : Created("register", result.Value);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginPerson([FromBody] PersonLoginDto loginModel)
        {
            var result = await personAuthService.AuthenticationPersonAsync(loginModel);
            return result.IsFailure ? result.ToProblemDetails() : Created("login", new { Token = result.Value });
        }
        
        [HttpPost("forgot-password")]
        public async Task<ActionResult<string>> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            var result = await personAuthService.ForgotPasswordAsync(forgotPassword);
            return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
        }
        [HttpPost("reset-password")]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            var result = await personAuthService.ResetPasswordAsync(resetPassword);
            return result.IsSuccess ? Ok(result) : result.ToProblemDetails();
        }
    }
}
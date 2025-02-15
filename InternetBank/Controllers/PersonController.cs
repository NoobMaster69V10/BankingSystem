using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers
{
    public class PersonController(IPersonAuthService personAuthService, IAccountTransactionService transactionService, IPersonService personService) : CustomControllerBase
    {
        [Authorize(Roles = "Person")]
        [HttpPost("transfer-money")]
        public async Task<IActionResult> TransferMoney(TransactionDto transactionDto)
        {
            var userId = User.FindFirst("personId")!.Value;
            var message = await transactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<IActionResult> GetPersonInfo()
        {
            var userId = User.FindFirst("personId")!.Value;

            var result = await personService.GetPersonById(userId);

            return Ok(result);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] PersonRegisterDto registerModel)
        {
            if (!await personAuthService.RegisterPersonAsync(registerModel))
            {
                return BadRequest("Invalid operation.");
            }
            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PersonLoginDto loginModel)
        {
            var result = await personAuthService.AuthenticationPersonAsync(loginModel);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new { result });
        }
    }
}

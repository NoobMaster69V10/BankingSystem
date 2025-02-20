using System.Runtime.InteropServices.JavaScript;
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
            var response = await transactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            if (!response.Success)
            {
                return BadRequest(new { ErrorMessage = response.Message });
            }

            return Ok(new { response.Message });
        }

        [Authorize(Roles = "Person")]
        [HttpGet("information")]
        public async Task<IActionResult> GetPersonInfo()
        {
            var personId = User.FindFirst("personId")!.Value;
            var response = await personService.GetPersonById(personId);

            return Ok(response.Data);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(PersonRegisterDto registerModel)
        {
            var response = await personAuthService.RegisterPersonAsync(registerModel);
            if (!response.Success)
            {
                return BadRequest(new { ErrorMessage = response.Message });
            }
            return Ok(new { response.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(PersonLoginDto loginModel)
        {
            var response = await personAuthService.AuthenticationPersonAsync(loginModel);
            if (!response.Success)
            {
                return BadRequest(new { ErrorMessage = response.Message });
            }
            return Ok(new{ Token = response.Data });
        }
    }
}

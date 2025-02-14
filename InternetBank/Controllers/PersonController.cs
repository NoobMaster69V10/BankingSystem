using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController(IAccountTransactionService transactionService, IPersonService personService) : ControllerBase
    {
        [Authorize(Roles = "Person")]
        [HttpPost("transfer-money")]
        public async Task<IActionResult> TransferMoney(TransactionDto transactionDto)
        {
            var userId = User.FindFirst("userId")!.Value;
            var message = await transactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Person")]
        [HttpGet("get-info")]
        public async Task<IActionResult> GetPersonInfo()
        {
            var userId = User.FindFirst("userId")!.Value;

            var result = await personService.GetPersonById(userId);

            return Ok(result);
        }
    }
}

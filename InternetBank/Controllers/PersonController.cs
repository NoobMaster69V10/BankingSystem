using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.ExternalApiContracts;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController(IAccountTransactionService transactionService) : ControllerBase
    {
        [HttpPost("transfer-money")]
        public async Task<IActionResult> TransferMoney(TransactionDto transactionDto)
        {
            var message = await transactionService.TransactionBetweenAccountsAsync(transactionDto);

            return BadRequest(new { Message = message });
        }
    }
}

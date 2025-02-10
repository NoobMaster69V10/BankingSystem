using BankingSystem.Core.DTO;
using BankingSystem.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace InternetBank.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(ITransactionService transactionService) : ControllerBase
    {

        [HttpPost("transfer-money")]
        public async Task<IActionResult> TransferMoney(TransactionDto transactionDto)
        {
            await transactionService.TransactionBetweenOwnAccountsAsync(transactionDto);

            return Ok(new { Message = "Transaction added successfully" });
        }

    }
}

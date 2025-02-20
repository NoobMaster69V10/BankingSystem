using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
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
        IPersonService personService) : ControllerBase
    {
        [Authorize(Roles = "Person")]
        [HttpPost("transfer-money")]
        public async Task<ActionResult<ApiResponse>> TransferMoney(TransactionDto transactionDto)
        {
            var userId = User.FindFirst("personId")!.Value;
            var message = await transactionService.TransactionBetweenAccountsAsync(transactionDto, userId);

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<ActionResult<ApiResponse>> GetPersonInfo()
        {
            var userId = User.FindFirst("personId")!.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse
                {
                    IsSuccess = false,
                    ErrorMessages = ["User ID not found"]
                });
            }

            var result = await personService.GetPersonById(userId);
            if (result == null)
            {
                return NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    ErrorMessages = ["User not found."]
                });
            }

            return Ok(result);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponse>> RegisterUser([FromBody] PersonRegisterDto registerModel)
        {
            var response = await personAuthService.RegisterPersonAsync(registerModel);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] PersonLoginDto loginModel)
        {
            var response = await personAuthService.AuthenticationPersonAsync(loginModel);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(new { response });
        }
    }
}
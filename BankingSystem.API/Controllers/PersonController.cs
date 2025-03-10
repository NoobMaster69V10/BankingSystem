using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.Extensions;
using BankingSystem.Core.Response;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController(
        IPersonAuthService personAuthService) : ControllerBase
    {
        
        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<ActionResult<Person>> GetPersonInfo([FromServices]IPersonService personService, CancellationToken cancellationToken)
        {
            var personId = User.FindFirst("personId")!.Value;

            var result = await personService.GetPersonById(personId, cancellationToken);
            return result.IsFailure ? result.ToProblemDetails() : Ok(result.Value);
        }

        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> RegisterUser(PersonRegisterDto registerModel)
        {
            var result = await personAuthService.RegisterPersonAsync(registerModel);
            return result.IsFailure ? result.ToProblemDetails() : Created("register", result.Value);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticatedResponse>> LoginPerson(PersonLoginDto loginModel)
        {
            var result = await personAuthService.AuthenticationPersonAsync(loginModel);
            return result.IsFailure ? result.ToProblemDetails() : Created("login", result.Value);
        }
        
        [HttpPost("forgot-password")]
        public async Task<ActionResult<string>> ForgotPassword(ForgotPasswordDto forgotPassword)
        {
            var result = await personAuthService.ForgotPasswordAsync(forgotPassword);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }
        [HttpPost("reset-password")]
        public async Task<ActionResult<bool>> ResetPassword(ResetPasswordDto resetPassword)
        {
            var result = await personAuthService.ResetPasswordAsync(resetPassword);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }

        [HttpGet("email-confirmation")]
        public async Task<ActionResult<string>> EmailConfirmation([FromQuery] string token,[FromQuery] string email)
        {
            var result = await personAuthService.EmailConfirmationAsync(token,email);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }
        
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthenticatedResponse>> RefreshToken([FromBody] RefreshTokenDto refreshToken)
        {
            var result = await personAuthService.RefreshTokenAsync(refreshToken);
            return result.IsFailure ? result.ToProblemDetails() : Created("refresh-token", result.Value);
        }
    }
}
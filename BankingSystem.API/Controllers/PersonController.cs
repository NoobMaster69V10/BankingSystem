using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.Response;
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
        IPersonAuthService personAuthService) : ControllerBase
    {
        
        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        public async Task<ActionResult<Person>> GetPersonInfo([FromServices]IPersonService personService)
        {
            var personId = User.FindFirst("personId")!.Value;

            var result = await personService.GetPersonById(personId);
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

        [HttpPost("email-confirmation")]
        public async Task<ActionResult<string>> EmailConfirmation(EmailConfirmationDto emailConfirmationDto)
        {
            var result = await personAuthService.EmailConfirmationAsync(emailConfirmationDto);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }
    }
}
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
    /// <summary>
    /// Manages customer accounts, authentication, and authorization.
    /// </summary>
    /// <remarks>
    /// This controller handles user registration, login, password management, and account verification.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonAuthService _personAuthService;
        private readonly IPersonService _personService;

        public PersonController(IPersonAuthService personAuthService, IPersonService personService)
        {
            _personAuthService = personAuthService;
            _personService = personService;
        }

        /// <summary>
        /// Retrieves personal information for the authenticated user.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The personal information of the authenticated user.</returns>
        /// <response code="200">Returns the user's personal information.</response>
        [Authorize(Roles = "Person")]
        [HttpGet("info")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        public async Task<ActionResult<Person>> GetPersonInfo(
            CancellationToken cancellationToken)
        {
            var personId = User.FindFirst("personId")!.Value;

            var result = await _personService.GetPersonById(personId, cancellationToken);
            return result.IsFailure ? result.ToProblemDetails() : Ok(result.Value);
        }

        /// <summary>
        /// Registers a new customer account.
        /// </summary>
        /// <param name="registerModel">The user registration details including name, email, and password.</param>
        /// <returns>Registration confirmation including the created account ID.</returns>
        /// <remarks>
        /// Only bank operators can register new customers.
        /// </remarks>
        /// <response code="201">Returns the registration confirmation.</response>
        [Authorize(Roles = "Operator")]
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult<RegisterResponse>> RegisterUser(PersonRegisterDto registerModel)
        {
            var result = await _personAuthService.RegisterPersonAsync(registerModel);
            return result.IsFailure ? result.ToProblemDetails() : Created("register", result.Value);
        }

        /// <summary>
        /// Authenticates a user and provides access tokens.
        /// </summary>
        /// <param name="loginModel">The login credentials including email and password.</param>
        /// <returns>Authentication response including access and refresh tokens.</returns>
        /// <response code="201">Returns the authentication tokens.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticatedResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult<AuthenticatedResponse>> LoginPerson(PersonLoginDto loginModel)
        {
            var result = await _personAuthService.AuthenticationPersonAsync(loginModel);
            return result.IsFailure ? result.ToProblemDetails() : Created("login", result.Value);
        }
        
        /// <summary>
        /// Initiates the password reset process for a forgotten password.
        /// </summary>
        /// <param name="forgotPassword">The email address associated with the account.</param>
        /// <returns>Confirmation message about the password reset email.</returns>
        /// <response code="200">Returns a confirmation message.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> ForgotPassword(ForgotPasswordDto forgotPassword)
        {
            var result = await _personAuthService.ForgotPasswordAsync(forgotPassword);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }

        /// <summary>
        /// Resets a user's password using a reset token.
        /// </summary>
        /// <param name="resetPassword">The password reset details including token, email, and new password.</param>
        /// <returns>Confirmation of password reset success.</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ResetPassword(ResetPasswordDto resetPassword)
        {
            var result = await _personAuthService.ResetPasswordAsync(resetPassword);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }

        /// <summary>
        /// Confirms a user's email address using a verification token.
        /// </summary>
        /// <param name="token">The email verification token.</param>
        /// <param name="email">The email address to confirm.</param>
        /// <returns>Confirmation message about email verification status.</returns>
        /// <response code="200">Returns confirmation of email verification.</response>
        [HttpGet("email-confirmation")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> EmailConfirmation([FromQuery] string token,[FromQuery] string email)
        {
            var result = await _personAuthService.EmailConfirmationAsync(token,email);
            return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
        }

        /// <summary>
        /// Refreshes the authentication token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token from a previous authentication.</param>
        /// <returns>New authentication tokens.</returns>
        /// /// <response code="201">Returns refresh token.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthenticatedResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult<AuthenticatedResponse>> RefreshToken(RefreshTokenDto refreshToken)
        {
            var result = await _personAuthService.RefreshTokenAsync(refreshToken);
            return result.IsFailure ? result.ToProblemDetails() : Created("refresh-token", result.Value);
        }
    }
}
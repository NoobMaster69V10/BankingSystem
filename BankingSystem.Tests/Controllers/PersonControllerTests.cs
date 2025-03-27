using System.Security.Claims;
using BankingSystem.API.Controllers;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.RefreshToken;
using BankingSystem.Core.Response;
using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Errors;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.Tests.Controllers
{
    public class PersonControllerTests
    {
        private readonly IPersonAuthService _personAuthService;
        private readonly IPersonService _personService;
        private readonly PersonController _controller;

        public PersonControllerTests()
        {
            _personAuthService = A.Fake<IPersonAuthService>();
            _personService = A.Fake<IPersonService>();
            _controller = new PersonController(_personAuthService, _personService);
        }

        private void SetupUserContext(string personId)
        {
            var claims = new[] { new Claim("personId", personId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetPersonInfo_ValidPersonId_ReturnsOkResult()
        {
            var personId = "test-person-id";
            SetupUserContext(personId);

            var expectedPerson = new Person { PersonId = personId };
            var expectedResult = Result<Person>.Success(expectedPerson);

            A.CallTo(() => _personService.GetPersonByIdAsync(
                    A<string>.That.Matches(id => id == personId),
                    A<CancellationToken>.Ignored))
                .Returns(Task.FromResult(expectedResult));

            var result = await _controller.GetPersonInfo(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedPerson, okResult.Value);
        }

        [Fact]
        public async Task GetPersonInfo_PersonNotFound_ReturnsProblemDetails()
        {
            var personId = "non-existent-id";
            SetupUserContext(personId);

            var expectedResult = Result<Person>.Failure(CustomError.NotFound("Person not found"));

            A.CallTo(() => _personService.GetPersonByIdAsync(personId, A<CancellationToken>._))
                .Returns(expectedResult);

            var result = await _controller.GetPersonInfo(CancellationToken.None);

            Assert.IsType<ObjectResult>(result.Result);
        }

        [Fact]
        public async Task RegisterUser_ValidRegistration_ReturnsCreatedResult()
        {
            var registerDto = new PersonRegisterDto
            {
                Email = "test@example.com",
                FirstName = "Test",
                Lastname = "User",
                IdNumber = "12345678901",
                BirthDate = new DateTime(1990, 1, 1),
                Password = "password123",
                ConfirmPassword = "password123",
                Role = Role.User
            };
            var expectedResponse = new RegisterResponse
            {
                FirstName = "Test",
                Lastname = "User",
                IdNumber = "12345678901",
                BirthDate = new DateTime(1990, 1, 1),
                Email = "test@example.com",
                Role = Role.User
            };
            var expectedResult = Result<RegisterResponse>.Success(expectedResponse);

            A.CallTo(() => _personAuthService.RegisterPersonAsync(
                    A<PersonRegisterDto>.That.Matches(dto =>
                        dto.Email == registerDto.Email &&
                        dto.FirstName == registerDto.FirstName),
                    A<CancellationToken>._))
                .Returns(Task.FromResult(expectedResult));

            var result = await _controller.RegisterUser(registerDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(expectedResponse, createdResult.Value);
            Assert.Equal("register", createdResult.Location);
        }

        [Fact]
        public async Task RegisterUser_InvalidRegistration_ReturnsProblemDetails()
        {
            var registerDto = new PersonRegisterDto
            {
                Email = "invalid-email"
            };
            var expectedResult =
                Result<RegisterResponse>.Failure(CustomError.Validation("Invalid registration details"));

            A.CallTo(() => _personAuthService.RegisterPersonAsync(registerDto, A<CancellationToken>._))
                .Returns(Task.FromResult(expectedResult));

            var result = await _controller.RegisterUser(registerDto);

            Assert.IsType<ObjectResult>(result.Result);
        }

        [Fact]
        public async Task LoginPerson_ValidCredentials_ReturnsCreatedResult()
        {
            var loginDto = new PersonLoginDto 
            { 
                Email = "test@example.com", 
                Password = "password123" 
            };
            var expectedResponse = new AuthenticatedResponse 
            { 
                Token = "test-access-token", 
                RefreshToken = "test-refresh-token" 
            };
            var expectedResult = Result<AuthenticatedResponse>.Success(expectedResponse);
        
            A.CallTo(() => _personAuthService.AuthenticationPersonAsync(loginDto, A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.LoginPerson(loginDto);
        
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(expectedResponse, createdResult.Value);
            Assert.Equal("login", createdResult.Location);
        }
        
        [Fact]
        public async Task LoginPerson_InvalidCredentials_ReturnsProblemDetails()
        {
            var loginDto = new PersonLoginDto 
            { 
                Email = "invalid@example.com", 
                Password = "wrongpassword" 
            };
            var expectedResult = Result<AuthenticatedResponse>.Failure(CustomError.Validation("Invalid login credentials"));
        
            A.CallTo(() => _personAuthService.AuthenticationPersonAsync(loginDto,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.LoginPerson(loginDto);
        
            Assert.IsType<ObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task ForgotPassword_ValidEmail_ReturnsOkResult()
        {
            var forgotPasswordDto = new ForgotPasswordDto 
            { 
                Email = "test@example.com" 
            };
            var expectedResult = Result<string>.Success("Password reset email sent");
        
            A.CallTo(() => _personAuthService.ForgotPasswordAsync(forgotPasswordDto,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.ForgotPassword(forgotPasswordDto);
        
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Password reset email sent", okResult.Value);
        }
        
        [Fact]
        public async Task ForgotPassword_InvalidEmail_ReturnsProblemDetails()
        {
            var forgotPasswordDto = new ForgotPasswordDto 
            { 
                Email = "nonexistent@example.com" 
            };
            var expectedResult = Result<string>.Failure(CustomError.NotFound("Email not found"));
        
            A.CallTo(() => _personAuthService.ForgotPasswordAsync(forgotPasswordDto,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.ForgotPassword(forgotPasswordDto);
        
            Assert.IsType<ObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task ResetPassword_ValidReset_ReturnsOkResult()
        {
            var resetPasswordDto = new ResetPasswordDto 
            { 
                Email = "test@example.com", 
                Token = "reset-token", 
                NewPassword = "newpassword123" 
            };
            var expectedResult = Result<bool>.Success(true);
        
            A.CallTo(() => _personAuthService.ResetPasswordAsync(resetPasswordDto,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.ResetPassword(resetPasswordDto);
        
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
        }
        
        [Fact]
        public async Task ResetPassword_InvalidReset_ReturnsProblemDetails()
        {
            var resetPasswordDto = new ResetPasswordDto 
            { 
                Email = "test@example.com", 
                Token = "invalid-token", 
                NewPassword = "newpassword123" 
            };
            var expectedResult = Result<bool>.Failure(CustomError.Validation("Invalid reset token"));
        
            A.CallTo(() => _personAuthService.ResetPasswordAsync(resetPasswordDto,A<CancellationToken>._))
                .Returns(expectedResult);

            var result = await _controller.ResetPassword(resetPasswordDto);
        
            Assert.IsType<ObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task EmailConfirmation_ValidToken_ReturnsOkResult()
        {
            var token = "valid-confirmation-token";
            var email = "test@example.com";
            var expectedResult = Result<string>.Success("Email confirmed successfully");
        
            A.CallTo(() => _personAuthService.EmailConfirmationAsync(token, email,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.EmailConfirmation(token, email);
        
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Email confirmed successfully", okResult.Value);
        }
        
        [Fact]
        public async Task EmailConfirmation_InvalidToken_ReturnsProblemDetails()
        {
            var token = "invalid-confirmation-token";
            var email = "test@example.com";
            var expectedResult = Result<string>.Failure(CustomError.Validation("Invalid confirmation token"));
        
            A.CallTo(() => _personAuthService.EmailConfirmationAsync(token, email,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.EmailConfirmation(token, email);
        
            Assert.IsType<ObjectResult>(result.Result);
        }
        
        [Fact]
        public async Task RefreshToken_ValidRefreshToken_ReturnsCreatedResult()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto 
            { 
                Token = "valid-refresh-token" 
            };
            var expectedResponse = new AuthenticatedResponse 
            { 
                Token = "new-access-token", 
                RefreshToken = "new-refresh-token" 
            };
            var expectedResult = Result<AuthenticatedResponse>.Success(expectedResponse);
        
            A.CallTo(() => _personAuthService.RefreshTokenAsync(refreshTokenDto,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.RefreshToken(refreshTokenDto);
        
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(expectedResponse, createdResult.Value);
            Assert.Equal("refresh-token", createdResult.Location);
        }
        
        [Fact]
        public async Task RefreshToken_InvalidRefreshToken_ReturnsProblemDetails()
        {
            var refreshTokenDto = new RefreshTokenDto 
            { 
                Token = "invalid-refresh-token" 
            };
            var expectedResult = Result<AuthenticatedResponse>.Failure(CustomError.Validation("Invalid refresh token"));
        
            A.CallTo(() => _personAuthService.RefreshTokenAsync(refreshTokenDto,A<CancellationToken>._))
                .Returns(expectedResult);
        
            var result = await _controller.RefreshToken(refreshTokenDto);
        
            Assert.IsType<ObjectResult>(result.Result);
        }
    }
}
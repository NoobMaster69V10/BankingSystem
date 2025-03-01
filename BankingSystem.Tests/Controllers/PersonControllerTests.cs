using BankingSystem.API.Controllers;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Person;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingSystem.Tests.Controllers;

public class PersonControllerTests
{
    private readonly IPersonAuthService _personAuthService;
    private readonly IAccountTransactionService _transactionService;
    private readonly IPersonService _personService;
    private readonly PersonController _controller;

    public PersonControllerTests()
    {
        _personAuthService = A.Fake<IPersonAuthService>();
        _transactionService = A.Fake<IAccountTransactionService>();
        _personService = A.Fake<IPersonService>();
        _controller = new PersonController(_personAuthService, _transactionService, _personService);
    }

    private void SetupUserContext(string personId)
    {
        var claims = new List<Claim>
        {
            new("personId", personId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region TransferMoney Tests

    [Fact]
    public async Task PersonController_TransferMoney_ReturnsCreatedResult()
    {
        string personId = "123";
        SetupUserContext(personId);

        var transactionDto = new AccountTransactionDto
        {
            Amount = 100,
            FromAccountId = 1,
            ToAccountId = 2,
        };

        var transactionResult = new AccountTransaction
        {
            Amount = transactionDto.Amount,
            FromAccountId = transactionDto.FromAccountId,
            ToAccountId = transactionDto.ToAccountId,
        };

        var successResult = Result<AccountTransaction>.Success(transactionResult);

        A.CallTo(() => _transactionService.TransactionBetweenAccountsAsync(transactionDto, personId))
            .Returns(Task.FromResult(successResult));

        var result = await _controller.TransferMoney(transactionDto);

        result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)result;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be("transfer-money");
        createdResult.Value.Should().Be(transactionResult);
    }

    [Fact]
    public async Task PersonController_TransferMoney_ReturnsBadRequest()
    {
        string personId = "123";
        SetupUserContext(personId);
    
        var transactionDto = new AccountTransactionDto
        {
            Amount = 100,
            FromAccountId = 1,
            ToAccountId = 2,
        };
    
        var error = CustomError.Validation("Insufficient funds for transfer");
        var failureResult = Result<AccountTransaction>.Failure(error);
    
        A.CallTo(() => _transactionService.TransactionBetweenAccountsAsync(transactionDto, personId))
            .Returns(Task.FromResult(failureResult));
    
        var result = await _controller.TransferMoney(transactionDto);
    
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(error);
    }
    
    #endregion
    
    #region GetPersonInfo Tests
    
    [Fact]
    public async Task PersonController_GetPersonInfo_ReturnsOkResult()
    {
        string personId = "123";
        SetupUserContext(personId);
    
        var person = new Person
        {
            PersonId = "ASdasds",
            FirstName = "wiko",
            Lastname = "kiko",
            Email = "wiko.kiko@example.com", 
            IdNumber = "12345678901",
            BirthDate = DateTime.Now,
        };
    
        var successResult = Result<Person>.Success(person);
    
        A.CallTo(() => _personService.GetPersonById(personId))
            .Returns(Task.FromResult(successResult));
    
        var result = await _controller.GetPersonInfo();
    
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(person);
    }
    
    [Fact]
    public async Task PersonController_GetPersonInfo_ReturnsBadRequest()
    {
        string personId = "999"; 
        SetupUserContext(personId);
    
        var error = CustomError.NotFound("Person not found");
        var failureResult = Result<Person>.Failure(error);
    
        A.CallTo(() => _personService.GetPersonById(personId))
            .Returns(Task.FromResult(failureResult));
    
        var result = await _controller.GetPersonInfo();
    
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(failureResult);
    }
    
    #endregion
    
    #region RegisterUser Tests
    
    [Fact]
    public async Task PersonControler_RegisterUser_ReturnsCreatedResult()
    {
        var registerModel = new PersonRegisterDto
        {
            Name = "wiko",
            Lastname = "asd",
            Email = "asd.asd@example.com",
            IdNumber = "12345678901", 
            BirthDate = DateTime.Now, 
            Password = "Password123!",
        };
    
        var successResult = Result<PersonRegisterDto>.Success(registerModel);
    
        A.CallTo(() => _personAuthService.RegisterPersonAsync(registerModel))
            .Returns(Task.FromResult(successResult));
    
        var result = await _controller.RegisterUser(registerModel);
    
        result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)result;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be("register");
        createdResult.Value.Should().Be(registerModel);
    }
    
    [Fact]
    public async Task PersonController_RegisterUser_ReturnsBadRequest()
    {
        var registerModel = new PersonRegisterDto
        {
            Name = "wiko",
            Lastname = "asd",
            Email = "asd.asd@example.com",
            IdNumber = "12345678901", 
            BirthDate = DateTime.Now, 
            Password = "Password123!",
        };
    
        var error = CustomError.Validation("Username already exists");
        var failureResult = Result<PersonRegisterDto>.Failure(error);
    
        A.CallTo(() => _personAuthService.RegisterPersonAsync(registerModel))
            .Returns(Task.FromResult(failureResult));
    
        var result = await _controller.RegisterUser(registerModel);
    
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(failureResult);
    }
    
    #endregion
    
    #region Login Tests
    
    [Fact]
    public async Task PersonController_Login__ReturnsCreatedResult()
    {
        var loginModel = new PersonLoginDto
        {
            Email = "wiko.kiko@example.com",
            Password = "Password123!"
        };
    
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var successResult = Result<string>.Success(token);
    
        A.CallTo(() => _personAuthService.AuthenticationPersonAsync(loginModel))
            .Returns(Task.FromResult(successResult));
    
        var result = await _controller.Login(loginModel);
    
        result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)result;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be("login");
        createdResult.Value.Should().BeEquivalentTo(new { Token = token });
    }
    
    [Fact]
    public async Task PersonController_Login_ReturnsBadRequest()
    {
        var loginModel = new PersonLoginDto
        {
            Email = "wiko.kiko@example.com",
            Password = "Password123!"
        };
    
        var error = CustomError.Failure("Invalid username or password");
        var failureResult = Result<string>.Failure(error);
    
        A.CallTo(() => _personAuthService.AuthenticationPersonAsync(loginModel))
            .Returns(Task.FromResult(failureResult));
    
        var result = await _controller.Login(loginModel);
    
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(failureResult);
    }
    
    #endregion
    
    #region ForgotPassword Tests
    
    [Fact]
    public async Task ForgotPassword_WhenSuccessful_ReturnsOkResult()
    {
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "john.doe@example.com"
        };
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var successResult = Result<string>.Success(token);
    
        A.CallTo(() => _personAuthService.ForgotPasswordAsync(forgotPasswordDto))
            .Returns(Task.FromResult(successResult));
    
        var result = await _controller.ForgotPassword(forgotPasswordDto);
    
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(successResult);
    }
    
    [Fact]
    public async Task ForgotPassword_WhenFailed_ReturnsBadRequest()
    {
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "nonexistent@example.com"
        };
        
        var error = CustomError.NotFound("Email not found");
        var failureResult = Result<string>.Failure(error);
    
        A.CallTo(() => _personAuthService.ForgotPasswordAsync(forgotPasswordDto))
            .Returns(Task.FromResult(failureResult));
    
        var result = await _controller.ForgotPassword(forgotPasswordDto);
    
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(error);
    }
    
    #endregion
    
    #region ResetPassword Tests
    
    [Fact]
    public async Task ResetPassword_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "john.doe@example.com",
            Token = "reset-token-123",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
    
        var successResult = Result<bool>.Success(true);
    
        A.CallTo(() => _personAuthService.ResetPasswordAsync(resetPasswordDto))
            .Returns(Task.FromResult(successResult));
    
        var result = await _controller.ResetPassword(resetPasswordDto);
    
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(successResult);
    }
    
    [Fact]
    public async Task ResetPassword_WhenFailed_ReturnsBadRequest()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "john.doe@example.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };
    
        var error = CustomError.Validation("Invalid or expired token");
        var failureResult = Result<bool>.Failure(error);
    
        A.CallTo(() => _personAuthService.ResetPasswordAsync(resetPasswordDto))
            .Returns(Task.FromResult(failureResult));
    
        var result = await _controller.ResetPassword(resetPasswordDto);
    
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be(error);
    }
    
    #endregion
}
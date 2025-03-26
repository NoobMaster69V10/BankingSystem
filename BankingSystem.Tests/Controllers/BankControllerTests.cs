using BankingSystem.API.Controllers;
using BankingSystem.Core.DTO.BankAccount;
using BankingSystem.Core.DTO.BankCard;
using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.Tests.Controllers;

public class BankControllerTests
{
    private readonly IBankAccountService _accountService;
    private readonly IBankCardService _cardService;
    private readonly BankController _controller;

    public BankControllerTests()
    {
        _accountService = A.Fake<IBankAccountService>();
        _cardService = A.Fake<IBankCardService>();
        _controller = new BankController(_accountService, _cardService);
    }
    
    [Fact]
    public async Task CreateBankAccount_WhenSuccessful_ReturnsCreatedResult()
    {
        var bankAccountDto = new BankAccountRegisterDto 
        { 
            Iban = "DE89370400440532013000",
            Balance = 1000m,
            Username = "kiko",
            Currency = Currency.EUR
        };
        
        var createdAccount = new BankAccount 
        { 
            BankAccountId = 1,
            Iban = "DE89370400440532013000",
            Balance = 1000m,
            PersonId = "kiko",
            Currency = Currency.EUR
        };
        
        var successResult = Result<BankAccount>.Success(createdAccount);
        
        A.CallTo(() => _accountService.CreateBankAccountAsync(bankAccountDto, default))
            .Returns(Task.FromResult(successResult));
        
        var result = await _controller.CreateBankAccount(bankAccountDto, default);
        
        result.Result.Should().BeOfType<CreatedResult>();
        var createdResult = result.Result as CreatedResult;
        createdResult!.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be("account");
        createdResult.Value.Should().Be(createdAccount);
    }
    
    [Fact]
    public async Task CreateBankAccount_WhenBankAccountExists_ReturnsBadRequest()
    {
        var bankAccountDto = new BankAccountRegisterDto 
        { 
            Iban = "DE89370400440532013000", 
            Balance = 1000m,
            Username = "johndoe",
            Currency = Currency.EUR
        };
        var customError = CustomError.Validation("Bank account already exists.");
        var expectedResult = Result<BankAccount>.Failure(customError); 
        
        A.CallTo(() => _accountService.CreateBankAccountAsync(bankAccountDto, default))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.CreateBankAccount(bankAccountDto, default); 
        
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(400);
        problemDetails.Title.Should().Be("Validation");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { customError });
    }
    [Fact]
    public async Task CreateBankCard_WhenAccountNotExist_ReturnNotFound()
    {
        var cardRegisterDto = new BankCardRegisterDto 
        { 
            Username = "wiko",
            CardNumber = "12345678",
            ExpirationDate = DateTime.Now.AddYears(1),
            Cvv = "123",
            PinCode = "1234",
            BankAccountId = 1
        };
        
        var expectedError = CustomError.NotFound("Bank account not found");
        var expectedResult = Result<BankCard>.Failure(expectedError);
        A.CallTo(() => _cardService.CreateBankCardAsync(cardRegisterDto, default))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.CreateBankCard(cardRegisterDto, default);
        
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(404);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { expectedError });
    }

    [Fact]
    public async Task CreateBankCard_WhenValidationFails_ReturnsBadRequest()
    {
        var cardRegisterDto = new BankCardRegisterDto 
        { 
            Username = "wiko",
            CardNumber = "12345678",
            ExpirationDate = DateTime.Now.AddYears(1),
            Cvv = "123",
            PinCode = "1234",
            BankAccountId = 1
        };
        var customError = CustomError.Failure("Bank card could not be created.");
        var expectedResult = Result<BankCard>.Failure(customError);
        A.CallTo(() => _cardService.CreateBankCardAsync(cardRegisterDto, default))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.CreateBankCard(cardRegisterDto, default);
        
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(500);
        problemDetails.Title.Should().Be("Failure");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { customError });
    }
}
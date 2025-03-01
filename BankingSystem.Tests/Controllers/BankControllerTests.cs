using BankingSystem.API.Controllers;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
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
            Username = "johndoe",
            Currency = "EUR"
        };
        
        var createdAccount = new BankAccount 
        { 
            BankAccountId = 1,
            IBAN = "DE89370400440532013000",
            Balance = 1000m,
            PersonId = "AASdasd",
            Currency = "EUR"
        };
        
        var successResult = Result<BankAccount>.Success(createdAccount);
        
        A.CallTo(() => _accountService.CreateBankAccountAsync(bankAccountDto))
            .Returns(Task.FromResult(successResult));
        
        var result = await _controller.CreateBankAccount(bankAccountDto);
        
        result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)result;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Location.Should().Be("account");
        createdResult.Value.Should().Be(createdAccount);
    }
    
     [Fact]
     public async Task BankController_CreateBankAccount_ReturnBadRequest_WhenValidationFails()
     {
         // Arrange
         var bankAccountDto = new BankAccountRegisterDto 
         { 
             Iban = "DE89370400440532013000",
             Balance = 1000m,
             Username = "johndoe",
             Currency = "EUR"
         };
         var customError = new CustomError("ValidationError", "Invalid account details provided");
         var expectedResult = Result<BankAccount>.Failure(customError);
         
         A.CallTo(() => _accountService.CreateBankAccountAsync(bankAccountDto))
             .Returns(Task.FromResult(expectedResult));
         
         var result = await _controller.CreateBankAccount(bankAccountDto);
         
         var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
         badRequestResult.StatusCode.Should().Be(400);
         badRequestResult.Value.Should().BeEquivalentTo(expectedResult);
     }

     [Fact]
     public async Task BankController_CreateBankCard_ReturnCreated()
     {
         var cardRegisterDto = new BankCardRegisterDto 
         { 
             Username = "wiko",
             CardNumber = "12345678",
             Firstname = "Luka",
             Lastname = "wiko",
             ExpirationDate = DateTime.Now.AddYears(1),
             Cvv = "123",
             PinCode = "1234",
             BankAccountId = 1
         };
         var expectedResponse = new BankCard 
         { 
             BankCardId = 1,
             Firstname = cardRegisterDto.Firstname,
             Lastname = cardRegisterDto.Lastname,
             CardNumber = cardRegisterDto.CardNumber,
             ExpirationDate = cardRegisterDto.ExpirationDate,
             Cvv = cardRegisterDto.Cvv,
             PinCode = cardRegisterDto.PinCode,
             AccountId = cardRegisterDto.BankAccountId
         };
         var expectedResult = Result<BankCard>.Success(expectedResponse);
         A.CallTo(() => _cardService.CreateBankCardAsync(cardRegisterDto))
             .Returns(Task.FromResult(expectedResult));
         
         var result = await _controller.CreateBankCard(cardRegisterDto);
         
         var createdResult = result.Should().BeOfType<CreatedResult>().Which;
         createdResult.StatusCode.Should().Be(201);
         createdResult.Value.Should().BeEquivalentTo(expectedResult.Value);
         createdResult.Location.Should().Be("card");
     }

     [Fact]
     public async Task BankController_CreateBankCard_ReturnBadRequest_WhenAccountNotFound()
     {
         var cardRegisterDto = new BankCardRegisterDto 
         { 
             Username = "wiko",
             CardNumber = "12345678",
             Firstname = "Luka",
             Lastname = "wiko",
             ExpirationDate = DateTime.Now.AddYears(1),
             Cvv = "123",
             PinCode = "1234",
             BankAccountId = 1
         };
         
         var expectedResult = Result<BankCard>.Failure(
             CustomError.NotFound("Account not found"));
         A.CallTo(() => _cardService.CreateBankCardAsync(cardRegisterDto))
             .Returns(Task.FromResult(expectedResult));
         
         var result = await _controller.CreateBankCard(cardRegisterDto);
         
         var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
         badRequestResult.StatusCode.Should().Be(400);
         badRequestResult.Value.Should().BeEquivalentTo(expectedResult);
     }

     [Fact]
     public async Task BankController_CreateBankCard_ReturnBadRequest_WhenValidationFails()
     {
         var cardRegisterDto = new BankCardRegisterDto 
         { 
             Username = "wiko",
             CardNumber = "12345678",
             Firstname = "Luka",
             Lastname = "wiko",
             ExpirationDate = DateTime.Now.AddYears(1),
             Cvv = "123",
             PinCode = "1234",
             BankAccountId = 1
         };
         var customError = new CustomError("ValidationError", "Invalid card details provided");
         var expectedResult = Result<BankCard>.Failure(customError);
         A.CallTo(() => _cardService.CreateBankCardAsync(cardRegisterDto))
             .Returns(Task.FromResult(expectedResult));
         
         var result = await _controller.CreateBankCard(cardRegisterDto);
         
         var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
         badRequestResult.StatusCode.Should().Be(400);
         badRequestResult.Value.Should().BeEquivalentTo(expectedResult);
     }
 }
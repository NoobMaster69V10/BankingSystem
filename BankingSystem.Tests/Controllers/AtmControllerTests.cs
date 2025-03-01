using BankingSystem.API.Controllers;
using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Response;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystem.Tests.Controllers;

public class AtmControllerTests
{
    private readonly IAtmService _atmService;
    private readonly IAccountTransactionService _accountTransactionService;
    private readonly AtmController _controller;

    public AtmControllerTests()
    {
        _accountTransactionService = A.Fake<IAccountTransactionService>();
        _atmService = A.Fake<IAtmService>();
        _controller = new AtmController(_atmService, _accountTransactionService);
    }

    [Fact]
    public async Task ShowBalance_WhenCardIsValid_Then_ReturnOk()
    {
        var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
        var expectedResponse = new BalanceResponseDto(balance: 500.00m, cardNumber: "123456789");
        var expectedResult = CustomResult<BalanceResponseDto>.Success(expectedResponse);
        A.CallTo(() => _atmService.ShowBalanceAsync(cardDto))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.ShowBalance(cardDto);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ShowBalance_WhenCardIsInvalid_Then_ReturnBadRequest()
    {
        var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" }; 
        var expectedResult = CustomResult<BalanceResponseDto>.Failure(CustomError.NotFound("Card number not found"));
        A.CallTo(() => _atmService.ShowBalanceAsync(cardDto))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.ShowBalance(cardDto);
        
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeEquivalentTo(expectedResult.Error);
    }

    [Fact]
    public async Task WithdrawMoney_WhenTransactionIsSuccessful_Then_ReturnOk()
    {
        var withdrawDto = new WithdrawMoneyDto {CardNumber = "5127 8809 9999 9990",PinCode = "1234",Amount = 500,Currency = "USD"};
        var expectedResult = CustomResult<bool>.Success(true);
        A.CallTo(() => _accountTransactionService.WithdrawMoneyAsync(withdrawDto))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.WithdrawMoney(withdrawDto);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task WithdrawMoney_WhenBalanceIsNotEnough_Then_ReturnBadRequest()
    {
        var withdrawDto = new WithdrawMoneyDto 
        { 
            CardNumber = "123456789", 
            PinCode = "1234", 
            Amount = 10000, 
            Currency = "USD" 
        };
        var customError = new CustomError("NotEnoughBalance", "Not enough balance.");
        var expectedResult = CustomResult<bool>.Failure(customError);
        A.CallTo(() => _accountTransactionService.WithdrawMoneyAsync(withdrawDto))
            .Returns(Task.FromResult(expectedResult));
    
        var result = await _controller.WithdrawMoney(withdrawDto);
    
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeEquivalentTo(customError);
    }

    [Fact]
    public async Task WithdrawMoney_WhenCardIsInvalid_Then_ReturnBadRequest()
    {
        var withdrawDto = new WithdrawMoneyDto 
        { 
            CardNumber = "invalid", 
            PinCode = "wrong", 
            Amount = 50, 
            Currency = "USD" 
        };
        var expectedResult = CustomResult<bool>.Failure(CustomError.NotFound("Card number not found"));
        A.CallTo(() => _accountTransactionService.WithdrawMoneyAsync(withdrawDto))
            .Returns(Task.FromResult(expectedResult));
    
        var result = await _controller.WithdrawMoney(withdrawDto);
    
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeEquivalentTo(expectedResult.Error);
    }
    [Fact]
    public async Task ChangePin_ValidRequest_ReturnsOk()
    {
        var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
        var resultDto = CustomResult<bool>.Success(true);

        A.CallTo(() => _atmService.ChangePinAsync(changePinDto)).Returns(resultDto);

        var result = await _controller.ChangePin(changePinDto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(resultDto);
    }

    [Fact]
    public async Task ChangePin_InvalidRequest_ReturnsProblemDetails()
    {
        var changePinDto = new ChangePinDto { CardNumber = "invalid", CurrentPin = "wrong", NewPin = "5678" };
        var customError = new CustomError("INVALID_CARD", "Card validation failed");
        var resultDto = CustomResult<bool>.Failure(customError);

        A.CallTo(() => _atmService.ChangePinAsync(changePinDto)).Returns(resultDto);

        var result = await _controller.ChangePin(changePinDto);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        objectResult.Value.Should().BeEquivalentTo(resultDto.ToProblemDetails());
    }
}

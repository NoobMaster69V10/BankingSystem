using BankingSystem.API.Controllers;
using BankingSystem.Core.DTO.AtmTransaction;
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
    private readonly AtmController _controller;

    public AtmControllerTests()
    {
        _atmService = A.Fake<IAtmService>();
        _controller = new AtmController(_atmService);
    }

    [Fact]
    public async Task ShowBalance_WhenValidC_ReturnOk()
    {
        var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
        var expectedResponse = new BalanceResponseDto(balance: 500.00m, cardNumber: "123456789");
        var expectedResult = Result<BalanceResponseDto>.Success(expectedResponse);
        
        A.CallTo(() => _atmService.ShowBalanceAsync(cardDto))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.ShowBalance(cardDto);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ShowBalance_WhenInvalidCard_ReturnsNotFound()
    {
        var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" }; 
        var expectedError = CustomError.NotFound("Card number not found");
        var expectedResult = Result<BalanceResponseDto>.Failure(expectedError);
    
        A.CallTo(() => _atmService.ShowBalanceAsync(cardDto))
            .Returns(Task.FromResult(expectedResult));

        var result = await _controller.ShowBalance(cardDto);

        var badRequestResult = result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        badRequestResult.StatusCode.Should().Be(404); 
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { expectedError });
    }

    [Fact]
    public async Task WithDrawMoney_WhenValidCard_ReturnOk()
    {
        var withdrawDto = new WithdrawMoneyDto {CardNumber = "5127 8809 9999 9990",PinCode = "1234",Amount = 500};
        var expectedResult = Result<bool>.Success(true);
        A.CallTo(() => _atmService.WithdrawMoneyAsync(withdrawDto))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.WithdrawMoney(withdrawDto);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task WithdrawMoney_WhenNotEnoughBalance_ReturnFailure()
    {
        var withdrawDto = new WithdrawMoneyDto 
        { 
            CardNumber = "123456789", 
            PinCode = "1234", 
            Amount = 10000, 
        };
        var customError = new CustomError("NotEnoughBalance", "Not enough balance.");
        var expectedResult = Result<bool>.Failure(customError);
        A.CallTo(() => _atmService.WithdrawMoneyAsync(withdrawDto))
            .Returns(Task.FromResult(expectedResult));
    
        var result = await _controller.WithdrawMoney(withdrawDto);
    
        var badRequestResult = result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = badRequestResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        badRequestResult.StatusCode.Should().Be(500); 
        problemDetails.Title.Should().Be("Failure");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { customError });
    }

    [Fact]
    public async Task WithdrawMoney_WhenInvalidCard_ReturnNotFound()
    {
        var withdrawDto = new WithdrawMoneyDto 
        { 
            CardNumber = "invalid", 
            PinCode = "wrong", 
            Amount = 50, 
        };
        var expectedError = CustomError.NotFound("Card number not found");
        var expectedResult = Result<bool>.Failure(expectedError);
        A.CallTo(() => _atmService.WithdrawMoneyAsync(withdrawDto))
            .Returns(Task.FromResult(expectedResult));
    
        var result = await _controller.WithdrawMoney(withdrawDto);
    
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(404);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { expectedError });
    }

    [Fact]
    public async Task ChangePin_WhenValidCard_ReturnOk()
    {
        var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
        var expectedResult = Result<bool>.Success(true);
        A.CallTo(() => _atmService.ChangePinAsync(changePinDto))
            .Returns(Task.FromResult(expectedResult));
        
        var result = await _controller.ChangePin(changePinDto);
        
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ChangePin_WhenInvalidCard_ReturnNotFound()
    {
        var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
        var expectedError = CustomError.NotFound("Card number not found");
        var expectedResult = Result<bool>.Failure(expectedError);
        A.CallTo(() => _atmService.ChangePinAsync(changePinDto))
            .Returns(Task.FromResult(expectedResult));
    
        var result = await _controller.ChangePin(changePinDto);
    
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        objectResult.StatusCode.Should().Be(404);
        problemDetails.Title.Should().Be("Not Found");
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(new[] { expectedError });
    }
}
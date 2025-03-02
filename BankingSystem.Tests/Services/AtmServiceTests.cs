using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using FakeItEasy;
using FluentAssertions;

namespace BankingSystem.Tests.Services
{
    public class AtmServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBankCardService _bankCardService;
        private readonly ILoggerService _loggerService;
        private readonly AtmService _atmService;
        private readonly IHasherService _hasherService;

        public AtmServiceTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _bankCardService = A.Fake<IBankCardService>();
            _loggerService = A.Fake<ILoggerService>();
            _hasherService = A.Fake<IHasherService>();
            _atmService = new AtmService(_unitOfWork, _bankCardService, _loggerService, _hasherService);
        }

        #region ShowBalanceAsync Tests

        [Fact]
        public async Task AtmService_ShowBalanceAsync_ValidCard_ReturnsBalance()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
            var validationResult = Result<bool>.Success(true);
            decimal expectedBalance = 1000.50m;

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber))
                .Returns(expectedBalance);

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Balance.Should().Be(expectedBalance);
            result.Value.CardNumber.Should().Be(cardDto.CardNumber);
        }

        [Fact]
        public async Task AtmService_ShowBalanceAsync_InvalidCard_ReturnsFailure()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "invalid", PinCode = "wrong" };
            var validationResult = Result<bool>.Failure(CustomError.NotFound("Card not found"));

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode))
                .Returns(validationResult);

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(validationResult.Error);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ShowBalanceAsync_ExceptionThrown_ReturnsFailure()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber))
                .Throws(new Exception("Database error"));

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("BALANCE_ERROR");
            A.CallTo(() => _loggerService.LogErrorInConsole(A<string>.That.Contains("Error in ShowBalanceAsync")))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region ChangePinAsync Tests

        [Fact]
        public async Task ChangePinAsync_ValidCard_ReturnsSuccess()
        {
            var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(validationResult);
            A.CallTo(() =>
                    _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.NewPin))
                .Returns(Task.CompletedTask);

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            A.CallTo(() =>
                    _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.NewPin))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ChangePinAsync_InvalidCard_ReturnsFailure()
        {
            var changePinDto = new ChangePinDto { CardNumber = "invalid", CurrentPin = "wrong", NewPin = "5678" };
            var customError = new CustomError("INVALID_CARD", "Card validation failed");
            var validationResult = Result<bool>.Failure(customError);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(validationResult);

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(customError);
            A.CallTo(() => _unitOfWork.BankCardRepository.UpdatePinAsync(A<string>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePinAsync_ExceptionThrown_ReturnsFailure()
        {
            var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(validationResult);
            A.CallTo(() =>
                    _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Throws(new Exception("Database error"));

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("PIN_CHANGE_ERROR");
            A.CallTo(() => _loggerService.LogErrorInConsole(A<string>.That.Contains("Error in ChangePinAsync")))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region AuthorizeCardAsync Tests

        [Fact]
        public async Task AuthorizeCardAsync_ValidCard_ReturnsSuccess()
        {
            string cardNumber = "123456789";
            string pin = "1234";
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardNumber, pin))
                .Returns(validationResult);

            var method = typeof(AtmService).GetMethod("AuthorizeCardAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<Result<bool>>)method.Invoke(_atmService, new object[] { cardNumber, pin });

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizeCardAsync_InvalidCard_ReturnsFailure()
        {
            string cardNumber = "invalid";
            string pin = "wrong";
            var customError = new CustomError("INVALID_CARD", "Card validation failed");
            var validationResult = Result<bool>.Failure(customError);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardNumber, pin))
                .Returns(validationResult);

            var method = typeof(AtmService).GetMethod("AuthorizeCardAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<Result<bool>>)method.Invoke(_atmService, new object[] { cardNumber, pin });

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(customError);
        }

        [Fact]
        public async Task AuthorizeCardAsync_ExceptionThrown_ReturnsFailure()
        {
            string cardNumber = "123456789";
            string pin = "1234";

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardNumber, pin))
                .Throws(new Exception("Service error"));

            var method = typeof(AtmService).GetMethod("AuthorizeCardAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<Result<bool>>)method.Invoke(_atmService, new object[] { cardNumber, pin });

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("AUTH_ERROR");
            A.CallTo(() => _loggerService.LogErrorInConsole(A<string>.That.Contains("Error in AuthorizeCardAsync")))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region WithDrawMoneyAsync Tests

        [Fact]
        public async Task WithdrawMoneyAsync_WhenAmountIsZeroOrNegative_()
        {
            var withdrawDto = new WithdrawMoneyDto { Amount = 0, CardNumber = "1234", PinCode = "0000" };
            
        }

        //     [Fact]
        //     public async Task WithdrawMoneyAsync_ShouldReturnFailure_WhenCardValidationFails()
        //     {
        //         var withdrawDto = new WithdrawMoneyDto { Amount = 100, CardNumber = "1234", PinCode = "0000" };
        //
        //         _bankCardServiceMock
        //             .Setup((System.Linq.Expressions.Expression<Func<IBankCardService, Task<Result<bool>>>>)(b =>
        //                 b.ValidateCardAsync("1234", "0000")))
        //             .ReturnsAsync(Result<bool>.Failure(CustomError.Validation("Invalid Card")));
        //
        //         var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);
        //
        //         Assert.False(result.IsSuccess);
        //         Assert.Equal("Invalid Card", result.Error.Message);
        //     }
        //
        //     [Fact]
        //     public async Task WithdrawMoneyAsync_ShouldReturnFailure_WhenNotEnoughBalance()
        //     {
        //         var withdrawDto = new WithdrawMoneyDto { Amount = 200, CardNumber = "1234", PinCode = "0000" };
        //
        //         _bankCardServiceMock
        //             .Setup((System.Linq.Expressions.Expression<Func<IBankCardService, Task<Result<bool>>>>)(b =>
        //                 b.ValidateCardAsync("1234", "0000")))
        //             .ReturnsAsync((Core.DTO.Result.Result<bool>)Result<bool>.Success(true));
        //
        //         _unitOfWorkMock.Setup(u => u.BankCardRepository.GetBalanceAsync("1234")).ReturnsAsync(100);
        //
        //         var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);
        //
        //         Assert.False(result.IsSuccess);
        //         Assert.Equal("Not enough balance.", result.Error.Message);
        //     }
        //
        //     [Fact]
        //     public async Task WithdrawMoneyAsync_ShouldReturnSuccess_WhenValid()
        //     {
        //         var withdrawDto = new WithdrawMoneyDto
        //             { Amount = 100, CardNumber = "1234", PinCode = "0000", Currency = "USD" };
        //         var bankAccount = new BankAccount { BankAccountId = 123, Balance = 500 };
        //
        //         _bankCardServiceMock
        //             .Setup((System.Linq.Expressions.Expression<Func<IBankCardService, Task<Result<bool>>>>)(b =>
        //                 b.ValidateCardAsync("1234", "0000")))
        //             .ReturnsAsync(Result<bool>.Success(true));
        //
        //         _unitOfWorkMock.Setup(u => u.BankCardRepository.GetBalanceAsync("1234")).ReturnsAsync(200);
        //         _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234")).ReturnsAsync(bankAccount);
        //
        //         _unitOfWorkMock
        //             .Setup(u => u.BankAccountRepository.UpdateBalanceAsync(bankAccount, 100))
        //             .Returns(Task.CompletedTask);
        //
        //         _unitOfWorkMock
        //             .Setup(u => u.TransactionRepository.AddAtmTransactionAsync(It.IsAny<AtmTransaction>()))
        //             .Returns(Task.CompletedTask);
        //
        //         var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);
        //
        //         Assert.True(result.IsSuccess);
        //     }
        // }

        #endregion
    }
}
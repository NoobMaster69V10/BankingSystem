using BankingSystem.Core.DTO;
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

        public AtmServiceTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _bankCardService = A.Fake<IBankCardService>();
            _loggerService = A.Fake<ILoggerService>();
            _atmService = new AtmService(_unitOfWork, _bankCardService, _loggerService);
        }

        #region ShowBalanceAsync Tests

        [Fact]
        public async Task AtmService_ShowBalanceAsync_ValidCard_ReturnsBalance()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
            var validationResult = CustomResult<bool>.Success(true);
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
            var validationResult = CustomResult<bool>.Failure(CustomError.RecordNotFound("Card not found"));

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
            var validationResult = CustomResult<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber))
                .Throws(new Exception("Database error"));

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("BALANCE_ERROR");
            A.CallTo(() => _loggerService.LogErrorInConsole(A<string>.That.Contains("Error in ShowBalanceAsync"))).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region ChangePinAsync Tests

        [Fact]
        public async Task ChangePinAsync_ValidCard_ReturnsSuccess()
        {
            var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
            var validationResult = CustomResult<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(Task.CompletedTask);

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            A.CallTo(() => _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ChangePinAsync_InvalidCard_ReturnsFailure()
        {
            var changePinDto = new ChangePinDto { CardNumber = "invalid", CurrentPin = "wrong", NewPin = "5678" };
            var customError = new CustomError("INVALID_CARD", "Card validation failed");
            var validationResult = CustomResult<bool>.Failure(customError);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(validationResult);

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(customError);
            A.CallTo(() => _unitOfWork.BankCardRepository.UpdatePinAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePinAsync_ExceptionThrown_ReturnsFailure()
        {
            var changePinDto = new ChangePinDto { CardNumber = "123456789", CurrentPin = "1234", NewPin = "5678" };
            var validationResult = CustomResult<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, changePinDto.CurrentPin))
                .Throws(new Exception("Database error"));

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("PIN_CHANGE_ERROR");
            A.CallTo(() => _loggerService.LogErrorInConsole(A<string>.That.Contains("Error in ChangePinAsync"))).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region AuthorizeCardAsync Tests

        [Fact]
        public async Task AuthorizeCardAsync_ValidCard_ReturnsSuccess()
        {
            string cardNumber = "123456789";
            string pin = "1234";
            var validationResult = CustomResult<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardNumber, pin))
                .Returns(validationResult);

            var method = typeof(AtmService).GetMethod("AuthorizeCardAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = await (Task<CustomResult<bool>>)method.Invoke(_atmService, new object[] { cardNumber, pin });

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task AuthorizeCardAsync_InvalidCard_ReturnsFailure()
        {
            string cardNumber = "invalid";
            string pin = "wrong";
            var customError = new CustomError("INVALID_CARD", "Card validation failed");
            var validationResult = CustomResult<bool>.Failure(customError);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardNumber, pin))
                .Returns(validationResult);

            var method = typeof(AtmService).GetMethod("AuthorizeCardAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = await (Task<CustomResult<bool>>)method.Invoke(_atmService, new object[] { cardNumber, pin });

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
            
            var result = await (Task<CustomResult<bool>>)method.Invoke(_atmService, new object[] { cardNumber, pin });

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("AUTH_ERROR");
            A.CallTo(() => _loggerService.LogErrorInConsole(A<string>.That.Contains("Error in AuthorizeCardAsync"))).MustHaveHappenedOnceExactly();
        }

        #endregion
    }
}
using FakeItEasy;
using FluentAssertions;
using BankingSystem.Core.DTO.AtmTransaction;
using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

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
            var currencyExchangeClient = A.Fake<ICurrencyExchangeClient>();
            _atmService = new AtmService(_unitOfWork, _bankCardService, _loggerService, _hasherService, currencyExchangeClient);
        }

        #region ShowBalanceAsync Tests

        [Fact]
        public async Task ShowBalanceAsync_ValidCard_ReturnsBalance()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
            var validationResult = Result<bool>.Success(true);
            var expectedBalance = 1000.50m;

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode, default))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber, default))
                .Returns(expectedBalance);

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Balance.Should().Be(expectedBalance);
            result.Value.CardNumber.Should().Be(cardDto.CardNumber);
        }

        [Fact]
        public async Task ShowBalanceAsync_InvalidCard_ReturnsFailure()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "invalid", PinCode = "wrong" };
            var validationResult = Result<bool>.Failure(CustomError.NotFound("Card not found"));

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode, default))
                .Returns(validationResult);

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(validationResult.Error);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(A<string>._, default)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ShowBalanceAsync_ExceptionThrown_ReturnsFailure()
        {
            var cardDto = new CardAuthorizationDto { CardNumber = "123456789", PinCode = "1234" };
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(cardDto.CardNumber, cardDto.PinCode, default))
                .Returns(validationResult);
            A.CallTo(() => _unitOfWork.BankCardRepository.GetBalanceAsync(cardDto.CardNumber, default))
                .Throws(new Exception("Database error"));

            var result = await _atmService.ShowBalanceAsync(cardDto);

            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be("BALANCE_ERROR");
            A.CallTo(() => _loggerService.LogError(A<string>.That.Contains("Error in ShowBalanceAsync")))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region ChangePinAsync Tests

        [Fact]
        public async Task ChangePinAsync_ValidCard_ReturnsSuccess()
        {
            var changePinDto = new ChangePinDto { CardNumber = "123456789", PinCode = "1234", NewPin = "5678" };
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.PinCode, default))
                .Returns(validationResult);

            A.CallTo(() => _hasherService.Hash(changePinDto.NewPin))
                .Returns("pin-hash");
            A.CallTo(() =>
                    _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, "pin-hash", default))
                .Returns(Task.CompletedTask);

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Pin changed Successfully");
        }

        [Fact]
        public async Task ChangePinAsync_InvalidCard_ReturnsFailure()
        {
            var changePinDto = new ChangePinDto { CardNumber = "invalid", PinCode = "wrong", NewPin = "5678" };
            var customError = new CustomError("INVALID_CARD", "Card validation failed");
            var validationResult = Result<bool>.Failure(customError);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.PinCode, default))
                .Returns(validationResult);

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(customError);
            A.CallTo(() => _unitOfWork.BankCardRepository.UpdatePinAsync(A<string>._, A<string>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePinAsync_ExceptionThrown_ReturnsFailure()
        {
            var changePinDto = new ChangePinDto { CardNumber = "123456789", PinCode = "1234", NewPin = "5678" };
            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(changePinDto.CardNumber, changePinDto.PinCode, default))
                .Returns(validationResult);

            A.CallTo(() => _hasherService.Hash(changePinDto.NewPin))
                .Returns("pin-hash");

            A.CallTo(() =>
                    _unitOfWork.BankCardRepository.UpdatePinAsync(changePinDto.CardNumber, "pin-hash", default))
                .Throws(new Exception("Database error"));

            var result = await _atmService.ChangePinAsync(changePinDto);

            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be("PIN_CHANGE_ERROR");
        }

        #endregion

        #region WithdrawMoneyAsync Tests

        [Fact]
        public async Task WithdrawMoneyAsync_ValidCard_ReturnsSuccess()
        {
            var withdrawDto = new WithdrawMoneyDto { Amount = 0, CardNumber = "1234", PinCode = "0000" };
            var bankAccount = new BankAccount { Balance = 1000m, Currency = Currency.GEL, BankAccountId = 1 };

            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(withdrawDto.CardNumber, withdrawDto.PinCode, default))
                .Returns(validationResult);

            A.CallTo(() => _unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawDto.CardNumber, A<CancellationToken>._))!
                .Returns(Task.FromResult(bankAccount));

            var result = await _atmService.WithdrawMoneyAsync(withdrawDto);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task WithdrawMoneyAsync_ShouldFail_WhenAccountNotFound()
        {
            var withdrawDto = new WithdrawMoneyDto { Amount = 0, CardNumber = "1234", PinCode = "0000" };

            var validationResult = Result<bool>.Success(true);

            A.CallTo(() => _bankCardService.ValidateCardAsync(withdrawDto.CardNumber, withdrawDto.PinCode, default))
                .Returns(validationResult);

            A.CallTo(() => _unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawDto.CardNumber, A<CancellationToken>._))
                .Returns((BankAccount)null!);

            var result = await _atmService.WithdrawMoneyAsync(withdrawDto);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task WithdrawMoneyAsync_ShouldFail_WhenInsufficientBalance()
        {
            var withdrawDto = new WithdrawMoneyDto { CardNumber = "123456789", PinCode = "1234", Amount = 2000 };
            var bankAccount = new BankAccount { Balance = 1000m, Currency = Currency.GEL, BankAccountId = 1 };

            A.CallTo(() => _bankCardService.ValidateCardAsync(withdrawDto.CardNumber, withdrawDto.PinCode, A<CancellationToken>._))
                .Returns(Task.FromResult(Result<bool>.Success(true)));
            A.CallTo(() => _unitOfWork.BankCardRepository.GetAccountByCardAsync(withdrawDto.CardNumber, A<CancellationToken>._))!
                .Returns(Task.FromResult(bankAccount));

            var result = await _atmService.WithdrawMoneyAsync(withdrawDto);

            result.IsFailure.Should().BeTrue();
        }

        #endregion

        #region DepositMoneyAsync Tests

        [Fact]
        public async Task DepositMoneyAsync_ShouldIncreaseBalance_WhenDepositIsMade()
        {
            var depositDto = new DepositMoneyDto { CardNumber = "123456789", PinCode = "1234", Amount = 500 };
            var bankAccount = new BankAccount { Balance = 1000m, Currency = Currency.GEL };

            A.CallTo(() => _bankCardService.ValidateCardAsync(depositDto.CardNumber, depositDto.PinCode, A<CancellationToken>._))
                .Returns(Task.FromResult(Result<bool>.Success(true)));
            A.CallTo(() => _unitOfWork.BankCardRepository.GetAccountByCardAsync(depositDto.CardNumber, A<CancellationToken>._))!
                .Returns(Task.FromResult(bankAccount));
            A.CallTo(() => _unitOfWork.BankAccountRepository.UpdateBalanceAsync(bankAccount, A<CancellationToken>._))
                .DoesNothing();

            var result = await _atmService.DepositMoneyAsync(depositDto);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Balance.Should().Be(1500m);
        }

        #endregion
    }
}
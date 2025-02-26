using BankingSystem.Core.DTO;
using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class AccountTransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExchangeRateApi> _exchangeRateApiMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly Mock<IBankCardService> _bankCardServiceMock;
    private readonly IAccountTransactionService _accountTransactionService;

    public AccountTransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _exchangeRateApiMock = new Mock<IExchangeRateApi>();
        _loggerServiceMock = new Mock<ILoggerService>();
        _bankCardServiceMock = new Mock<IBankCardService>();

        _accountTransactionService = new AccountTransactionService(
            _unitOfWorkMock.Object,
            _exchangeRateApiMock.Object,
            _loggerServiceMock.Object,
            _bankCardServiceMock.Object
        );
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenSameAccount()
    {
        var transactionDto = new TransactionDto { FromAccountId = 123, ToAccountId = 123, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("It is not possible to make a transaction between the same accounts.", result.Error.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenFromAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123)).ReturnsAsync((BankAccount)null);

        var transactionDto = new TransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account with id '123' not found!", result.Error.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenInsufficientBalance()
    {
        var fromAccount = new BankAccount { BankAccountId = 123, Balance = 50, Currency = "USD", PersonId = "user1" };
        var toAccount = new BankAccount { BankAccountId = 456, Balance = 200, Currency = "USD", PersonId = "user2" };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123)).ReturnsAsync(fromAccount);
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(456)).ReturnsAsync(toAccount);

        var transactionDto = new TransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient balance for this transaction.", result.Error.Message);
    }

    [Fact]
    public async Task WithdrawMoneyAsync_ShouldReturnFailure_WhenAmountIsZeroOrNegative()
    {
        var withdrawDto = new WithdrawMoneyDto { Amount = 0, CardNumber = "1234", Pin = "0000" };

        var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Amount must be greater than 0.", result.Error.Message);
    }

    [Fact]
    public async Task WithdrawMoneyAsync_ShouldReturnFailure_WhenCardValidationFails()
    {
        var withdrawDto = new WithdrawMoneyDto { Amount = 100, CardNumber = "1234", Pin = "0000" };

        _bankCardServiceMock
            .Setup(b => b.ValidateCardAsync("1234", "0000"))
            .ReturnsAsync(CustomResult<bool>.Failure(CustomError.Validation("Invalid Card")));

        var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid Card", result.Error.Message);
    }

    [Fact]
    public async Task WithdrawMoneyAsync_ShouldReturnFailure_WhenNotEnoughBalance()
    {
        var withdrawDto = new WithdrawMoneyDto { Amount = 200, CardNumber = "1234", Pin = "0000" };

        _bankCardServiceMock
            .Setup(b => b.ValidateCardAsync("1234", "0000"))
            .ReturnsAsync(CustomResult<bool>.Success(true));

        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetBalanceAsync("1234")).ReturnsAsync(100);

        var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Not enough balance.", result.Error.Message);
    }

    [Fact]
    public async Task WithdrawMoneyAsync_ShouldReturnSuccess_WhenValid()
    {
        var withdrawDto = new WithdrawMoneyDto { Amount = 100, CardNumber = "1234", Pin = "0000", Currency = "USD" };
        var bankAccount = new BankAccount { BankAccountId = 123, Balance = 500 };

        _bankCardServiceMock
            .Setup(b => b.ValidateCardAsync("1234", "0000"))
            .ReturnsAsync(CustomResult<bool>.Success(true));

        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetBalanceAsync("1234")).ReturnsAsync(200);
        _unitOfWorkMock.Setup(u => u.BankCardRepository.GetAccountByCardAsync("1234")).ReturnsAsync(bankAccount);

        _unitOfWorkMock
            .Setup(u => u.BankAccountRepository.UpdateBalanceAsync(bankAccount, 100))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.TransactionRepository.AddAtmTransactionAsync(It.IsAny<AtmTransaction>()))
            .Returns(Task.CompletedTask);

        var result = await _accountTransactionService.WithdrawMoneyAsync(withdrawDto);

        Assert.True(result.IsSuccess);
    }
}

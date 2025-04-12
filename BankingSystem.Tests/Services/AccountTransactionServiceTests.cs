using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.ConfigurationSettings.AccountTransaction;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Options;
using Moq;

namespace BankingSystem.Tests.Services;

public class AccountTransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExchangeService> _exchangeServiceMock;
    private readonly IAccountTransactionService _accountTransactionService;
    private readonly Mock<IOptions<AccountTransactionSettings>> _accountTransactionSettingsMock = new Mock<IOptions<AccountTransactionSettings>>();

    public AccountTransactionServiceTests()
    {
        _accountTransactionSettingsMock.Setup(x => x.Value).Returns(new AccountTransactionSettings
        {
            TransferFeeToAnotherAccount = 5.0m,
            TransferCommissionToAnotherAccount = 0.02m
        });

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _exchangeServiceMock = new Mock<IExchangeService>();
        Mock<ILoggerService> loggerServiceMock = new();

        _accountTransactionService = new AccountTransactionService(
            _unitOfWorkMock.Object,
            _exchangeServiceMock.Object,
            _accountTransactionSettingsMock.Object,
            loggerServiceMock.Object
        );
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_FromAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync((BankAccount)null!);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 123, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account not found!", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenPersonIdIsIncorrect()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync(new BankAccount());

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("You don't have permission to make transactions from this account.", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenToAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync(new BankAccount{PersonId = "user1"});

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(456, default)).ReturnsAsync((BankAccount)null!);

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account You want to transfer not found!", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenBalanceIsNotEnough()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync(new BankAccount { PersonId = "user1", Balance = 100});

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 500 };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(456, default)).ReturnsAsync(new BankAccount { PersonId = "user2", Balance = 100});

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient balance for this transaction.", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenUnexpectedError()
    {
        var fromAccount = new BankAccount { PersonId = "user1", Balance = 100000, Currency = Currency.GEL };
        var toAccount = new BankAccount { PersonId = "user2", Balance = 10000, Currency = Currency.GEL };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync(fromAccount);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 500 };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(456, default)).ReturnsAsync(toAccount);


        _exchangeServiceMock
            .Setup(es => es.ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency, default))
            .ReturnsAsync(20m);

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred during the transaction.", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnSuccess_WhenIsCorrect()
    {
        var fromAccount = new BankAccount { PersonId = "user1", Balance = 100000, Currency = Currency.GEL };
        var toAccount = new BankAccount { PersonId = "user2", Balance = 100000, Currency = Currency.GEL };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync(fromAccount);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 500 };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(456, default)).ReturnsAsync(toAccount);


        _exchangeServiceMock
            .Setup(es => es.ConvertCurrencyAsync(transactionDto.Amount, fromAccount.Currency, toAccount.Currency, default))
            .ReturnsAsync(20m);

        _unitOfWorkMock.Setup(u => u.BankTransactionRepository.TransferBetweenAccountsAsync(It.IsAny<AccountTransfer>(), 20m, default));

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.True(result.IsSuccess);
    }
}
using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class AccountTransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IAccountTransactionService _accountTransactionService;

    public AccountTransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<IExchangeService> exchangeServiceMock = new();
        Mock<ILoggerService> loggerServiceMock = new();
        
        _accountTransactionService = new AccountTransactionService(
            _unitOfWorkMock.Object,
            exchangeServiceMock.Object,
            loggerServiceMock.Object
        );
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenSameAccount()
    {
        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 123, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("It is not possible to make a transaction between the same accounts.", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenFromAccountNotFound()
    {
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync((BankAccount)null!);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account with id '123' not found!", result.Error!.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenInsufficientBalance()
    {
        var fromAccount = new BankAccount { BankAccountId = 123, Balance = 50, Currency = Currency.USD, PersonId = "user1" };
        var toAccount = new BankAccount { BankAccountId = 456, Balance = 200, Currency = Currency.USD, PersonId = "user2" };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(123, default)).ReturnsAsync(fromAccount);
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetAccountByIdAsync(456, default)).ReturnsAsync(toAccount);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient balance for this transaction.", result.Error!.Message);
    }
}
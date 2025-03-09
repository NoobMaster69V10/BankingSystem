using BankingSystem.Core.DTO.AccountTransaction;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.ExternalApiContracts;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests.Services;

public class AccountTransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrencyExchangeClient> _exchangeRateApiMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly IAccountTransactionService _accountTransactionService;

    public AccountTransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _exchangeRateApiMock = new Mock<ICurrencyExchangeClient>();
        _loggerServiceMock = new Mock<ILoggerService>();

        _accountTransactionService = new AccountTransactionService(
            _unitOfWorkMock.Object,
            _exchangeRateApiMock.Object,
            _loggerServiceMock.Object
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
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetByIdAsync(123)).ReturnsAsync((BankAccount)null);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Bank account with id '123' not found!", result.Error.Message);
    }

    [Fact]
    public async Task TransactionBetweenAccountsAsync_ShouldReturnFailure_WhenInsufficientBalance()
    {
        var fromAccount = new BankAccount { BankAccountId = 123, Balance = 50, Currency = "USD", PersonId = "user1" };
        var toAccount = new BankAccount { BankAccountId = 456, Balance = 200, Currency = "USD", PersonId = "user2" };

        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetByIdAsync(123)).ReturnsAsync(fromAccount);
        _unitOfWorkMock.Setup(u => u.BankAccountRepository.GetByIdAsync(456)).ReturnsAsync(toAccount);

        var transactionDto = new AccountTransactionDto { FromAccountId = 123, ToAccountId = 456, Amount = 100 };

        var result = await _accountTransactionService.TransactionBetweenAccountsAsync(transactionDto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient balance for this transaction.", result.Error.Message);
    }
}
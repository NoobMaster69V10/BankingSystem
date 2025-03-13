using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace BankingSystem.Tests.Services;

public class PersonServiceTests
{

    private readonly IPersonService _personService;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;


    public PersonServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        Mock<ILoggerService> loggerServiceMock = new();
        Mock<IMemoryCache> memoryCacheMock = new();
        _personService = new PersonService(
            _unitOfWorkMock.Object,
            memoryCacheMock.Object,
            loggerServiceMock.Object
        );
    }


    [Fact]
    public async Task GetPersonById_ShouldReturnFailure_WhenPersonNotFound()
    {
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByIdAsync("1", default)).ReturnsAsync((Person)null!);
        var result = await _personService.GetPersonById("1");
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error!.Message);
    }

    [Fact]
    public async Task GetPersonById_ShouldReturnSuccess_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetByIdAsync("1", default)).ReturnsAsync(new Person());
        var result = await _personService.GetPersonById("1");
        Assert.True(result.IsSuccess);
    }
}

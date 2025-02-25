using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.UnitOfWorkContracts;
using Moq;

namespace BankingSystem.Tests;

public class PersonServiceTests
{

    private readonly IPersonService _personService;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;


    public PersonServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerServiceMock = new Mock<ILoggerService>();
        _personService = new PersonService(
            _unitOfWorkMock.Object,
            _loggerServiceMock.Object
        );
    }


    [Fact]
    public async Task GetPersonById_ShouldReturnFailure_WhenPersonNotFound()
    {
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetPersonByIdAsync("1")).ReturnsAsync((Person)null);
        var result = await _personService.GetPersonById("1");
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error.Message);
    }

    [Fact]
    public async Task GetPersonById_ShouldReturnSuccess_WhenIsValid()
    {
        _unitOfWorkMock.Setup(u => u.PersonRepository.GetPersonByIdAsync("1")).ReturnsAsync(new Person());
        var result = await _personService.GetPersonById("1");
        Assert.True(result.IsSuccess);
    }
}

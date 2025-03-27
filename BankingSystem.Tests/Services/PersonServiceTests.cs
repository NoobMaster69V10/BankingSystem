using BankingSystem.Core.ServiceContracts;
using BankingSystem.Core.Services;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace BankingSystem.Tests.Services;

public class PersonServiceTests
{

    private readonly IPersonService _personService;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMemoryCache> _memoryCacheMock;


    public PersonServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _memoryCacheMock = new Mock<IMemoryCache>();
        Mock<ILoggerService> loggerServiceMock = new();
        _personService = new PersonService(
            _unitOfWorkMock.Object,
            _memoryCacheMock.Object,
            loggerServiceMock.Object
        );
    }


    

    [Fact]
    public async Task GetPersonById_ShouldReturnFailure_WhenPersonDoesNotExist()
    {
        var personId = "123";
        object cachedPerson = null!;

        _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedPerson!)).Returns(false);
        _unitOfWorkMock.Setup(x => x.PersonRepository.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person)null!);
        
        var result = await _personService.GetPersonByIdAsync(personId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomError.NotFound("User not found").Code, result.Error!.Code);
    }

    [Fact]
    public async Task GetPersonById_ShouldReturnFailure_WhenExceptionOccurs()
    {
        var personId = "123";
        object cachedPerson = null!;

        _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedPerson!)).Returns(false);
        _unitOfWorkMock.Setup(x => x.PersonRepository.GetByIdAsync(personId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _personService.GetPersonByIdAsync(personId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CustomError.Failure("Error occurred while getting person").Code, result.Error!.Code);
    }

    [Fact]
    public async Task GetPersonById_ShouldReturnPersonFromCache_WhenPersonIsCached()
    {
        var personId = "123";
        var expectedPerson = new Person { PersonId = personId, FirstName = "name" };
        object cachedPerson = expectedPerson;

        _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedPerson!)).Returns(true);

        var result = await _personService.GetPersonByIdAsync(personId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPerson, result.Value);
    }
}

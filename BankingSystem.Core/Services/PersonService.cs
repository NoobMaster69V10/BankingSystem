using BankingSystem.Core.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Caching.Memory;

namespace BankingSystem.Core.Services;

public class PersonService : IPersonService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILoggerService _loggerService;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(60);

    public PersonService(IUnitOfWork unitOfWork, IMemoryCache cache, ILoggerService loggerService)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _loggerService = loggerService;
    }

    public async Task<Result<Person>> GetPersonById(string personId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = "GetPersonInfo";
            if (_cache.TryGetValue(cacheKey, out Person? cachedReport))
            {
                if(cachedReport!.PersonId == personId)
                    return Result<Person>.Success(cachedReport);
            }
            
            var person = await _unitOfWork.PersonRepository.GetByIdAsync(personId, cancellationToken);

            if (string.IsNullOrEmpty(personId) || person is null)
            {
                return Result<Person>.Failure(CustomError.NotFound("User not found"));
            }
            _cache.Set(cacheKey, person, _cacheDuration);
            return Result<Person>.Success(person);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex.Message);
            return Result<Person>.Failure(CustomError.Failure("Error occurred while getting person"));
        }
    }
}

using BankingSystem.Core.DTO.Result;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using Microsoft.Extensions.Caching.Memory;

namespace BankingSystem.Core.Services;

public class PersonService(IUnitOfWork unitOfWork, ILoggerService loggerService, IMemoryCache cache) : IPersonService
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(60);

    public async Task<Result<Person>> GetPersonById(string personId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = "GetPersonInfo";
            if (cache.TryGetValue(cacheKey, out Person? cachedReport))
            {
                return Result<Person>.Success(cachedReport);
            }
            
            var person = await unitOfWork.PersonRepository.GetByIdAsync(personId, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cancellation requested");
            }

            if (string.IsNullOrEmpty(personId) || person is null)
            {
                return Result<Person>.Failure(CustomError.NotFound("User not found"));
            }
            cache.Set(cacheKey, person, _cacheDuration);
            return Result<Person>.Success(person);
        }
        catch (Exception ex)
        {
            loggerService.LogError(ex.Message);
            return Result<Person>.Failure(CustomError.Failure("Error occurred while getting person"));
        }
    }
}

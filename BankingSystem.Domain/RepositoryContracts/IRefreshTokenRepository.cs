using BankingSystem.Domain.Entities;

namespace BankingSystem.Domain.RepositoryContracts;

public interface IRefreshTokenRepository
{
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetDataByToken(string refreshToken,CancellationToken cancellationToken = default);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<string?> CheckPersonIdAsync(string personId, CancellationToken cancellationToken = default);
}
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    IDbConnection _connection;

    public RefreshTokenRepository(
        IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        const string query = "INSERT INTO RefreshTokens (Token, PersonId, ExpiresOnUtc) VALUES (@Token, @PersonId, @ExpiresOnUtc)";

        var parameters = new CommandDefinition(query, refreshToken, cancellationToken: cancellationToken);

        await _connection.ExecuteAsync(parameters);
    }

    public async Task<RefreshToken?> GetDataByToken(string refreshToken,CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM RefreshTokens WHERE Token = @Token";
        var parameters = new CommandDefinition(query, new {Token = refreshToken}, cancellationToken: cancellationToken);
        return await _connection.QueryFirstOrDefaultAsync<RefreshToken?>(parameters);
    }

    public Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        const string query ="Update RefreshTokens SET Token = @Token,ExpiresOnUtc = @ExpiresOnUtc WHERE PersonId = @PersonId";
        var parameters = new CommandDefinition(query, refreshToken, cancellationToken: cancellationToken);
        return _connection.ExecuteAsync(parameters);

    }

    public async Task<string?> CheckPersonIdAsync(string personId, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT PersonId FROM RefreshTokens WHERE PersonId = @PersonId";
        var parameters = new CommandDefinition(query, new {PersonId = personId}, cancellationToken: cancellationToken);
        return await _connection.QueryFirstOrDefaultAsync<string>(parameters);
    }
}
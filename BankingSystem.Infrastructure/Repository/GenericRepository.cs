using Dapper;
using System.Data;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly IDbConnection Connection;
    protected IDbTransaction? Transaction;
    private readonly string _tableName;

    public GenericRepository(IDbConnection connection, string tableName)
    {
        Connection = connection;
        _tableName = tableName;
    }

    public void SetTransaction(IDbTransaction? transaction)
    {
        Transaction = transaction;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = $"SELECT * FROM {_tableName}";
        return await Connection.QueryAsync<T>(query, transaction: Transaction);
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var query = $"SELECT * FROM {_tableName} WHERE {typeof(T).Name}Id = @Id";
        return await Connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id }, transaction: Transaction);
    }

    public async Task<int> AddAsync(T entity)
    {
        var query = GenerateInsertQuery();
        return await Connection.ExecuteAsync(query, entity, transaction: Transaction);
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var query = GenerateUpdateQuery();
        return await Connection.ExecuteAsync(query, entity, transaction: Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var query = $"DELETE FROM {_tableName} WHERE Id = @Id";
        return await Connection.ExecuteAsync(query, new { Id = id }, transaction: Transaction) > 0;
    }

    

    private string GenerateInsertQuery()
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name.ToLower() != $"{typeof(T).Name.ToLower()}id" && p.Name.ToLower() != "id")
            .Select(p => p.Name);
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"{typeof(T).Name.ToLower()}id");
        var columnNames = string.Join(", ", properties);
        var paramNames = string.Join(", ", properties.Select(p => "@" + p));

        return $"INSERT INTO {_tableName} ({columnNames}) VALUES ({paramNames})";
    }

    private string GenerateUpdateQuery()
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name.ToLower().ToLower() != $"{typeof(T).Name.ToLower()}id" && p.Name.ToLower() != "id")
            .Select(p => $"{p.Name} = @{p.Name}");

        var setClause = string.Join(", ", properties);

        return $"UPDATE {_tableName} SET {setClause} WHERE {typeof(T).Name}Id = @{typeof(T).Name}Id";
    }
}


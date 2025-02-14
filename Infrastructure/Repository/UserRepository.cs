using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

public class UserRepository : IUserRepository
{

    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;

    public UserRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task<Person?> GetUserByUsernameAsync(string username)
    {
        throw new NotImplementedException();
    }
    //public async Task<Person?> GetUserByUsernameAsync(string username)
    //{
    //    const string query = "SELECT * FROM AspNetUsers WHERE UserName = @Username";

    //    var userDictionary = new Dictionary<string, Person>();

    //    var users = await _connection.QueryAsync<Person, BankAccount, Person>(
    //        query,
    //        (user, bankAccount) =>
    //        {
    //            if (!userDictionary.TryGetValue(user.Id, out var currentUser))
    //            {
    //                currentUser = user;
    //                currentUser.BankAccounts = new List<BankAccount>();
    //                userDictionary.Add(currentUser.Id, currentUser);
    //            }
    //            if (bankAccount != null!)
    //                currentUser.BankAccounts.Add(bankAccount);
    //            return currentUser;
    //        },
    //        new { Username = username },
    //        splitOn: "Id",
    //    transaction: _transaction);

    //    return users.FirstOrDefault();
    //}

    // public async Task<Person?> GetUserByUsernameAsync(string username)
    // {
    //     const string query = @"
    //         SELECT u.Id, u.[Name], u.LastName, u.Email, u.IdNumber, u.BirthDate,
    //                b.Id, b.IBAN, b.Balance, b.Currency
    //         FROM [BankingSystem].[dbo].[AspNetUsers] u
    //         LEFT JOIN BankAccounts b ON u.Id = b.PersonId
    //         WHERE u.UserName = @Username";
    //
    //     var userDictionary = new Dictionary<string, Person>();
    //
    //     var users = await _connection.QueryAsync<Person, BankAccount, Person>(
    //         query,
    //         (user, bankAccount) =>
    //         {
    //             if (!userDictionary.TryGetValue(user.Id, out var currentUser))
    //             {
    //                 currentUser = user;
    //                 currentUser.BankAccounts = new List<BankAccount>();
    //                 userDictionary.Add(currentUser.Id, currentUser);
    //             }
    //             if (bankAccount != null!)
    //                 currentUser.BankAccounts!.Add(bankAccount);
    //             return currentUser;
    //         },
    //         new { Username = username },
    //         splitOn: "Id",
    //         transaction: _transaction
    //     );
    //
    //     return users.FirstOrDefault();
    // }
}
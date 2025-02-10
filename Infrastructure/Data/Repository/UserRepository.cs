using Dapper;
using Microsoft.Data.SqlClient;
using BankingSystem.Core.Identity;
using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Data.Repository;

public class UserRepository(SqlConnection conn) : IUserRepository
{
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        const string query = "SELECT * FROM AspNetUsers WHERE UserName = @Username";

        var userDictionary = new Dictionary<string, User>();

        var users = await conn.QueryAsync<User, BankAccount, User>(
            query,
            (user, bankAccount) =>
            {
                if (!userDictionary.TryGetValue(user.Id, out var currentUser))
                {
                    currentUser = user;
                    currentUser.BankAccounts = new List<BankAccount>();
                    userDictionary.Add(currentUser.Id, currentUser);
                }
                if (bankAccount != null!)
                    currentUser.BankAccounts.Add(bankAccount);
                return currentUser;
            },
            new { Username = username },
            splitOn: "Id"
        );

        return users.FirstOrDefault();
    }

    //public async Task<User?> GetUserByUsernameAsync(string username)
    //{
    //    const string query = @"
    //        SELECT u.Id, u.Firstname, u.LastName, u.Email, u.IdNumber, u.BirthDate 
    //               b.Id, b.IBAN, b.Balance, b.Currency
    //        FROM AspNetUsers u
    //        LEFT JOIN BankAccounts b ON u.Id = b.UserId
    //        WHERE u.UserName = @Username";

    //    var userDictionary = new Dictionary<string, User>();

    //    var users = await conn.QueryAsync<User, BankAccount, User>(
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
    //        splitOn: "Id"
    //    );

    //    return users.FirstOrDefault();
    //}
}
using System.Data;
using Dapper;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Data.Repository;

public class PersonRepository : IPersonRepository
{

    private readonly IDbConnection _connection;
    private IDbTransaction _transaction = null!;

    public PersonRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }
    public async Task<Person?> GetUserByIdAsync(string id)
    {
        const string query = @"
                         SELECT u.Id as PersonID, u.[Name], u.LastName, u.Email, u.IdNumber, u.BirthDate,
                         b.Id as BankAccountID, b.IBAN, b.Balance, b.Currency, b.PersonId,
                         bc.Id as BankCardID, bc.Firstname, bc.Lastname, bc.CardNumber, bc.ExpirationDate, bc.PinCode, bc.CVV, bc.PersonId
                         FROM [BankingSystem].[dbo].[AspNetUsers] u
                         LEFT JOIN BankAccounts b ON u.Id = b.PersonId
                         LEFT JOIN BankCards bc ON u.Id = bc.PersonId
                         WHERE u.Id = @ID;";

        var userDictionary = new Dictionary<string, Person>();

        var users = await _connection.QueryAsync<Person, BankAccount, BankCard, Person>(
            query,
            (person, bankAccount, bankCard) =>
            {
                if (!userDictionary.TryGetValue(person.PersonId, out var currentUser))
                {
                    currentUser = person;
                    currentUser.BankAccounts = new List<BankAccount>();
                    currentUser.Cards = new List<BankCard>();
                    userDictionary.Add(currentUser.PersonId, currentUser);
                }

                if (bankAccount != null! && currentUser.BankAccounts!.All(a => a.BankAccountId != bankAccount.BankAccountId))
                    currentUser.BankAccounts!.Add(bankAccount);

                if (bankCard != null! && currentUser.Cards!.All(c => c.BankCardId != bankCard.BankCardId))
                    currentUser.Cards!.Add(bankCard);

                return currentUser;
            },
            new { ID = id },
            splitOn: "BankAccountID,BankCardID",
            transaction: _transaction);

        return users.FirstOrDefault();
    }


    public async Task<Person?> GetUserByUsernameAsync(string username)
    {
        const string query = @"
            SELECT Id FROM [BankingSystem].[dbo].[AspNetUsers] WHERE u.UserName = @Username";

        var users = await _connection.QueryFirstOrDefaultAsync<Person>(query, _transaction);

        return users;
    }
}
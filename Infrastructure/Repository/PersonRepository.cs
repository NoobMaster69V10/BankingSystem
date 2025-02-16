using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using Dapper;

namespace BankingSystem.Infrastructure.Repository;

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
    public async Task<Person?> GetPersonByIdAsync(string id)
    {
        const string query = @"
                         SELECT u.Id as PersonID, u.FirstName, u.LastName, u.Email, u.IdNumber, u.BirthDate,
                            b.Id as BankAccountID, b.IBAN, b.Balance, b.Currency, b.PersonId,
                            bc.Id as BankCardID, bc.Firstname, bc.Lastname, bc.CardNumber, bc.ExpirationDate, bc.PinCode, bc.CVV, bc.AccountId
                            FROM AspNetUsers u
                            JOIN BankAccounts b ON u.Id = b.PersonId
                            LEFT JOIN BankCards bc ON b.Id = bc.AccountId
                         WHERE u.Id = @ID";

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

                if (bankAccount != null! && currentUser.BankAccounts!.All(a => a.Id != bankAccount.Id))
                    currentUser.BankAccounts!.Add(bankAccount);

                if (bankCard != null! && currentUser.Cards!.All(c => c.Id != bankCard.Id))
                    currentUser.Cards!.Add(bankCard);

                return currentUser;
            },
            new { ID = id },
            splitOn: "BankAccountID,BankCardID",
            transaction: _transaction);

        return users.FirstOrDefault();
    }
    public async Task<Person?> GetPersonByUsernameAsync(string username)
    {
        const string query = @"SELECT Id AS PersonId,FirstName,Lastname,IdNumber,Email,BirthDate FROM AspNetUsers WHERE Username = @Username";

        var result = await _connection.QueryFirstOrDefaultAsync<Person>(query, new { Username = username }, _transaction);

        return result;
    }
}
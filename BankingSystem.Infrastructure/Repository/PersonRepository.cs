using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class PersonRepository : GenericRepository<Person>, IPersonRepository
{
    public PersonRepository(IDbConnection connection) : base(connection, "AspNetUsers") { }
    public async Task<Person?> GetByIdAsync(string id)
    {
        const string query = @"
                         SELECT u.Id as PersonID, u.FirstName, u.LastName, u.Email, u.IdNumber, u.BirthDate,
                            b.BankAccountId, b.IBAN, b.Balance, b.Currency, b.PersonId,
                            bc.BankCardId, bc.Firstname, bc.Lastname, bc.CardNumber, bc.ExpirationDate, bc.PinCode, bc.CVV, bc.AccountId
                            FROM AspNetUsers u
                            LEFT JOIN BankAccounts b ON u.Id = b.PersonId
                            LEFT JOIN BankCards bc ON b.BankAccountId = bc.AccountId
                         WHERE u.Id = @ID";

        var userDictionary = new Dictionary<string, Person>();

        var users = await Connection.QueryAsync<Person, BankAccount, BankCard, Person>(
            query,
            (person, bankAccount, bankCard) =>
            {
                if (!userDictionary.TryGetValue(person.PersonId!, out var currentUser))
                {
                    currentUser = person;
                    currentUser.BankAccounts = new List<BankAccount>();
                    currentUser.Cards = new List<BankCard>();
                    userDictionary.Add(currentUser.PersonId!, currentUser);
                }

                if (currentUser.BankAccounts!.All(a => a.BankAccountId != bankAccount.BankAccountId))
                    currentUser.BankAccounts!.Add(bankAccount);

                if (currentUser.Cards!.All(c => c.BankCardId != bankCard.BankCardId))
                    currentUser.Cards!.Add(bankCard);
                
                return currentUser;
            },
            new { ID = id },
            splitOn: "BankAccountId,BankCardId",
            transaction: Transaction);

        return users.FirstOrDefault();
    }
    public async Task<Person?> GetByUsernameAsync(string username)
    {
        const string query = @"
                         SELECT u.Id as PersonID, u.FirstName, u.LastName, u.Email, u.IdNumber, u.BirthDate,
                            b.BankAccountId, b.IBAN, b.Balance, b.Currency, b.PersonId,
                            bc.BankCardId, bc.Firstname, bc.Lastname, bc.CardNumber, bc.ExpirationDate, bc.PinCode, bc.CVV, bc.AccountId
                            FROM AspNetUsers u
                            LEFT JOIN BankAccounts b ON u.Id = b.PersonId
                            LEFT JOIN BankCards bc ON b.BankAccountId = bc.AccountId
                         WHERE u.Username = @Username";

        var userDictionary = new Dictionary<string, Person>();

        var users = await Connection.QueryAsync<Person, BankAccount, BankCard, Person>(
            query,
            (person, bankAccount, bankCard) =>
            {
                if (!userDictionary.TryGetValue(person.PersonId!, out var currentUser))
                {
                    currentUser = person;
                    currentUser.BankAccounts = new List<BankAccount>();
                    currentUser.Cards = new List<BankCard>();
                    userDictionary.Add(currentUser.PersonId!, currentUser);
                }

                if (currentUser.BankAccounts!.All(a => a.BankAccountId != bankAccount.BankAccountId))
                    currentUser.BankAccounts?.Add(bankAccount);

                if (currentUser.Cards!.All(c => c.BankCardId != bankCard.BankCardId))
                    currentUser.Cards!.Add(bankCard);

                return currentUser;
            },
            new { Username = username },
            splitOn: "BankAccountId,BankCardId",
            transaction: Transaction);

        return users.FirstOrDefault();
    }
}

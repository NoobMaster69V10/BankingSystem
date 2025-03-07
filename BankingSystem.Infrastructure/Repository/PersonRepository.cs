using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class PersonRepository : RepositoryBase, IPersonRepository
{
    public PersonRepository(IDbConnection connection) : base(connection) { }
    public async Task<Person?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT u.Id as PersonID, u.FirstName, u.LastName, u.Email, u.IdNumber, u.BirthDate,
               b.BankAccountId, b.IBAN, b.Balance, b.Currency, b.PersonId,
               bc.BankCardId, bc.CardNumber, bc.ExpirationDate, bc.PinCode, bc.CVV, bc.AccountId
        FROM AspNetUsers u
        LEFT JOIN BankAccounts b ON u.Id = b.PersonId
        LEFT JOIN BankCards bc ON b.BankAccountId = bc.AccountId
        WHERE u.Id = @ID";
        
        var parameters = new CommandDefinition(query, new { ID = id }, transaction: Transaction,
            cancellationToken: cancellationToken);

        var userDictionary = new Dictionary<string, Person>();

        var users = await Connection.QueryAsync<Person, BankAccount, BankCard, Person>(
            parameters,
            (person, bankAccount, bankCard) =>
            {
                if (!userDictionary.TryGetValue(person.PersonId, out var currentUser))
                {
                    currentUser = person;
                    currentUser.BankAccounts = new List<BankAccount>();
                    currentUser.Cards = new List<BankCard>();
                    userDictionary.Add(currentUser.PersonId, currentUser);
                }

                if (bankAccount != null! &&
                    currentUser.BankAccounts.All(a => a.BankAccountId != bankAccount.BankAccountId))
                {
                    currentUser.BankAccounts.Add(bankAccount);
                }

                if (bankCard != null! &&
                    currentUser.Cards.All(c => c.BankCardId != bankCard.BankCardId))
                {
                    currentUser.Cards.Add(bankCard);
                }

                return currentUser;
            },
            splitOn: "BankAccountId,BankCardId");

        return users.FirstOrDefault();
    }

    public async Task<Person?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        const string query = @"
                         SELECT u.Id as PersonID, u.FirstName, u.LastName, u.Email, u.IdNumber, u.BirthDate,
                            b.BankAccountId, b.IBAN, b.Balance, b.Currency, b.PersonId,
                            bc.BankCardId,bc.CardNumber, bc.ExpirationDate, bc.PinCode, bc.CVV, bc.AccountId
                            FROM AspNetUsers u
                            LEFT JOIN BankAccounts b ON u.Id = b.PersonId
                            LEFT JOIN BankCards bc ON b.BankAccountId = bc.AccountId
                         WHERE u.Username = @Username";

        var parameters = new CommandDefinition(query, new { Username = username }, transaction: Transaction,
            cancellationToken: cancellationToken);

        var userDictionary = new Dictionary<string, Person>();

        var users = await Connection.QueryAsync<Person, BankAccount, BankCard, Person>(
            parameters,
            (person, bankAccount, bankCard) =>
            {
                if (!userDictionary.TryGetValue(person.PersonId, out var currentUser))
                {
                    currentUser = person;
                    currentUser.BankAccounts = new List<BankAccount>();
                    currentUser.Cards = new List<BankCard>();
                    userDictionary.Add(currentUser.PersonId, currentUser);
                }

                if (bankAccount != null! && currentUser.BankAccounts.All(a => a.BankAccountId != bankAccount.BankAccountId))
                    currentUser.BankAccounts.Add(bankAccount);

                if (bankCard != null! && currentUser.Cards.All(c => c.BankCardId != bankCard.BankCardId))
                    currentUser.Cards.Add(bankCard);

                return currentUser;
            },
            splitOn: "BankAccountId,BankCardId");

        return users.FirstOrDefault();
    }
}

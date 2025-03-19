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
        const string query = "SELECT * FROM vw_PersonInfo WHERE PersonID = @ID";

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
                    if (bankAccount.BankAccountStatus)
                    {
                        currentUser.BankAccounts.Add(bankAccount);
                    }
                }

                if (bankCard != null! &&
                    currentUser.Cards.All(c => c.BankCardId != bankCard.BankCardId))
                {
                    if (bankCard.BankCardStatus)
                    {
                        currentUser.Cards.Add(bankCard);
                    }
                }

                return currentUser;
            },
            splitOn: "BankAccountId,BankCardId");

        return users.FirstOrDefault();
    }

    public async Task<Person?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        const string query = "SELECT * FROM vw_PersonInfo WHERE Username = @Username";

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
                {
                    if (bankAccount.BankAccountStatus)
                    {
                        currentUser.BankAccounts.Add(bankAccount);
                    }
                }

                if (bankCard != null! && currentUser.Cards.All(c => c.BankCardId != bankCard.BankCardId))
                {
                    if (bankCard.BankCardStatus)
                    {
                        currentUser.Cards.Add(bankCard);
                    }
                }

                return currentUser;
            },
            splitOn: "BankAccountId,BankCardId");

        return users.FirstOrDefault();
    }
}

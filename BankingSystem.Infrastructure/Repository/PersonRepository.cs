using Dapper;
using System.Data;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;

namespace BankingSystem.Infrastructure.Repository;

public class PersonRepository : RepositoryBase, IPersonRepository
{
    public PersonRepository(IDbConnection connection) : base(connection) { }

    public Task<Person?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        GetPersonAsync("SELECT * FROM vw_PersonInfo WHERE PersonID = @Param", new { Param = id }, cancellationToken);

    public Task<Person?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        GetPersonAsync("SELECT * FROM vw_PersonInfo WHERE Username = @Param", new { Param = username }, cancellationToken);

    private async Task<Person?> GetPersonAsync(string query, object parameters, CancellationToken cancellationToken)
    {
            var commandDefinition = new CommandDefinition(
                query,
                parameters,
                transaction: Transaction,
                cancellationToken: cancellationToken
            );

            var userDictionary = new Dictionary<string, Person>();

            var users = await Connection.QueryAsync<Person, BankAccount, BankCard, Person>(
                commandDefinition,
                (person, bankAccount, bankCard) =>
                    MapPersonRelationships(person, bankAccount, bankCard, userDictionary),
                splitOn: "BankAccountId,BankCardId"
            );

            return users.FirstOrDefault();
    }

    private static Person MapPersonRelationships(
        Person person, 
        BankAccount bankAccount, 
        BankCard bankCard, 
        Dictionary<string, Person> userDictionary)
    {
        if (!userDictionary.TryGetValue(person.PersonId, out var currentUser))
        {
            currentUser = person;
            currentUser.BankAccounts = new List<BankAccount>();
            currentUser.Cards = new List<BankCard>();
            userDictionary.Add(currentUser.PersonId, currentUser);
        }

        if (bankAccount is not null && 
            currentUser.BankAccounts.All(a => a.BankAccountId != bankAccount.BankAccountId))
        {
            if (bankAccount.BankAccountStatus)
            {
                currentUser.BankAccounts.Add(bankAccount);
            }
        }

        if (bankCard is not null && 
            currentUser.Cards.All(c => c.BankCardId != bankCard.BankCardId))
        {
            if (bankCard.BankCardStatus)
            {
                currentUser.Cards.Add(bankCard);
            }
        }

        return currentUser;
    }
}
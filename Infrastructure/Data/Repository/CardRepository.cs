using BankingSystem.Core.Domain.Entities;
using BankingSystem.Core.Domain.RepositoryContracts;
using BankingSystem.Core.DTO;
using BankingSystem.Core.Identity;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BankingSystem.Infrastructure.Data.Repository;

public class CardRepository(SqlConnection conn) : ICardRepository
{
    public async Task CreateCardAsync(Card card)
    {
        const string query =
            "INSERT INTO Cards(Name, Lastname, CardNumber, ExpirationDate, CVV, PinCode, UserId) VALUES (@Name, @Lastname, @CardNumber, @ExpirationDate, @CVV, @PinCode, @UserId)";

        await conn.ExecuteAsync(query, new { Name = card.Name, Lastname = card.Lastname, CardNumber = card.CardNumber, ExpirationDate = card.ExpirationDate, CVV = card.CVV, PinCode = card.PinCode, UserId = card.UserId });
    }
}
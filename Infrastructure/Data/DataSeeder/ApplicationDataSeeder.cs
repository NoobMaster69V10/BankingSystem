using BankingSystem.Core.DTO;
using BankingSystem.Core.Helpers;
using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BankingSystem.Infrastructure.Data.DataSeeder;

public class ApplicationDataSeeder(
    BankingSystemDbContext context,
    UserManager<IdentityPerson> userManager,
    RoleManager<IdentityRole> roleManager,
    IBankAccountRepository bankAccountRepository,
    IBankCardRepository bankCardRepository,
    IPersonRepository personRepository,
    IConfiguration configuration,
    ILoggerService logger)
{
    public async Task Seed()
    {
        await SeedData();
    }

    private async Task SeedData()
    {
        try
        {
            await SeedRoles();
            await SeedUsersAndAccounts();

            logger.LogSuccessInConsole("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogErrorInConsole($"Error seeding database: {ex.Message}");
        }
    }

    private async Task SeedRoles()
    {
        var roles = new[] { "Operator", "Person" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedUsersAndAccounts()
    {
        if (context.IdentityPersons.Any()) return;

        var operatorPassword = configuration["Seeder:OperatorPassword"] ?? "Password1#";
        var personPassword = configuration["Seeder:PersonPassword"] ?? "Password1#";

        var users = new List<(string email, string role)>
        {
            ("test@gmail.com", "Operator"),
            ("testperson1@gmail.com", "Person"),
            ("testperson2@gmail.com", "Person")
        };

        foreach (var (email, role) in users)
        {
            var user = new IdentityPerson
            {
                UserName = email,
                Email = email,
                FirstName = email.Split('@')[0],
                Lastname = "User",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                IdNumber = "01010034023"
            };

            await userManager.CreateAsync(user, role == "Operator" ? operatorPassword : personPassword);

            await userManager.AddToRoleAsync(user, role);

            var person = await personRepository.GetPersonByUsernameAsync(email);

            var bankAccount = new BankAccount
            {
                Currency = "GEL",
                PersonId = person!.PersonId,
                Balance = 5000,
                IBAN = GenerateIban()
            };

            await bankAccountRepository.CreateAccountAsync(bankAccount);

            var personFullInfo = await personRepository.GetPersonByIdAsync(person.PersonId);

            var personAccountId = personFullInfo!.BankAccounts.First().BankAccountId;

            var (hashedPin, hashedCvv, salt) = HashingHelper.HashPinAndCvv(GeneratePinCode(), GenerateCvv());

            var card = new BankCard
            {
                CardNumber = GenerateCardNumber(),
                Cvv = hashedCvv,
                PinCode = hashedPin,
                Salt = salt,
                ExpirationDate = DateTime.UtcNow.AddYears(5),
                Firstname = user.FirstName,
                Lastname = user.Lastname,
                AccountId = personAccountId
            };

            await bankCardRepository.CreateCardAsync(card);
        }
    }

    private string GenerateIban() => $"GE{new Random().Next(100000000, 999999999)}{new Random().Next(100000000, 999999999)}";
    private string GenerateCardNumber() => $"{new Random().Next(100000000, 999999999)}{new Random().Next(10000, 99999)}";
    private string GenerateCvv() => new Random().Next(100, 999).ToString();
    private string GeneratePinCode() => new Random().Next(1000, 9999).ToString();
}

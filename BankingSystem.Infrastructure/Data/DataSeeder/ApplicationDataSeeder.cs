using BankingSystem.Core.DTO.BankCard;
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
    IHasherService hasherService,
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
        var roles = new[] { "Operator", "Person", "Manager" };

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
        var managerPassword = configuration["Seeder:ManagerPassword"] ?? "Password1#";
        var personPassword = configuration["Seeder:PersonPassword"] ?? "Password1#";

        var users = new List<(string email, string role)>
        {
            ("testOperator@gmail.com", "Operator"),
            ("testManager@gmail.com", "Manager"),
            ("testperson1@gmail.com", "Person"),
            ("testperson2@gmail.com", "Person")
        };

        foreach (var (email, role) in users)
        {
            var user = new IdentityPerson
            {
                UserName = email,
                Email = email,
                FirstName = email.Split("@")[0],
                Lastname = "",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                IdNumber = "01010034023"
            };

            switch (role)
            {
                case "Operator":
                    await userManager.CreateAsync(user, operatorPassword);
                    await userManager.AddToRoleAsync(user, role);
                    user.Lastname = "Operator";
                    break;
                case "Manager":
                    await userManager.CreateAsync(user, managerPassword);
                    await userManager.AddToRoleAsync(user, role);
                    user.Lastname = "Manager";
                    break;
                case "Person":
                    await userManager.CreateAsync(user, personPassword);
                    await userManager.AddToRoleAsync(user, role);
                    user.Lastname = "Person";
                    break;
            }

            var person = await personRepository.GetByUsernameAsync(email);

            var bankAccount = new BankAccount
            {
                Currency = "GEL",
                PersonId = person!.PersonId,
                Balance = 5000,
                IBAN = GenerateIban()
            };

            await bankAccountRepository.AddAsync(bankAccount);

            var personFullInfo = await personRepository.GetByIdAsync(person.PersonId);

            var personAccountId = personFullInfo!.BankAccounts.First().BankAccountId;

            
            var pinHash =
                hasherService.Hash(GeneratePinCode());


            var encryptedCvv = EncryptionHelper.Encrypt(GenerateCvv());

            var card = new BankCard
            {
                CardNumber = GenerateCardNumber(),
                Cvv = encryptedCvv,
                PinCode = pinHash,
                ExpirationDate = DateTime.UtcNow.AddYears(5),
                Firstname = user.FirstName,
                Lastname = user.Lastname,
                AccountId = personAccountId
            };

            await bankCardRepository.AddAsync(card);
        }
    }

    private string GenerateIban() => $"GE{new Random().Next(100000000, 999999999)}{new Random().Next(100000000, 999999999)}";
    private string GenerateCardNumber() => $"{new Random().Next(100000000, 999999999)}{new Random().Next(10000, 99999)}";
    private string GenerateCvv() => new Random().Next(100, 999).ToString();
    private string GeneratePinCode() => new Random().Next(1000, 9999).ToString();
}

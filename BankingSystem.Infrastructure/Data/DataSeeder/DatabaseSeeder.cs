using BankingSystem.Core.Identity;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.ConfigurationSettings.Seeder;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using BankingSystem.Domain.Helpers;
using BankingSystem.Domain.RepositoryContracts;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BankingSystem.Infrastructure.Data.DataSeeder;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly IOptions<SeederSettings> _seederSettings;
    private readonly BankingSystemDbContext _context;
    private readonly UserManager<IdentityPerson> _userManager;
    private readonly IPersonRepository _personRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankCardRepository _bankCardRepository;
    private readonly IHasherService _hasherService;
    private readonly ILoggerService _logger;

    public DatabaseSeeder(IHasherService hasherService,
        IOptions<SeederSettings> seederSettings, BankingSystemDbContext context,
        UserManager<IdentityPerson> userManager, IBankAccountRepository bankAccountRepository,
        IBankCardRepository bankCardRepository, IPersonRepository personRepository, ILoggerService logger)
    {
        _seederSettings = seederSettings;
        _context = context;
        _userManager = userManager;
        _personRepository = personRepository;
        _bankAccountRepository = bankAccountRepository;
        _bankCardRepository = bankCardRepository;
        _hasherService = hasherService;
        _logger = logger;
    }

    private SeederSettings SeederSettings => _seederSettings.Value;
    public async Task SeedDataAsync()
    {
        if (_context.IdentityPersons.Any())
        {
            _logger.LogSuccess("Database already seeded!");
            return;
        }

        var operatorPassword = SeederSettings.OperatorPassword;
        var managerPassword = SeederSettings.ManagerPassword;
        var personPassword = SeederSettings.PersonPassword;

        var users = new List<(string email, Role role)>
        {
            (SeederSettings.OperatorEmail, Role.Operator),
            (SeederSettings.ManagerEmail, Role.Manager),
            (SeederSettings.PersonEmail1, Role.User),
            (SeederSettings.PersonEmail2, Role.User)
        };


        foreach (var (email, role) in users)
        {
            var user = new IdentityPerson
            {
                UserName = email,
                Email = email,
                FirstName = email.Split("@")[0],
                Lastname = email.Split("@")[1],
                BirthDate = DateTime.UtcNow.AddYears(-30),
                IdNumber = GenerateIdNumber()
            };

            switch (role)
            {
                case Role.Operator:
                    await _userManager.CreateAsync(user, operatorPassword);
                    await _userManager.AddToRoleAsync(user, role.ToString());
                    await _userManager.AddToRoleAsync(user, "User");
                    user.Lastname = "Operator";
                    break;
                case Role.Manager:
                    await _userManager.CreateAsync(user, managerPassword);
                    await _userManager.AddToRoleAsync(user, role.ToString());
                    await _userManager.AddToRoleAsync(user, "User");
                    user.Lastname = "Manager";
                    break;
                case Role.User:
                    await _userManager.CreateAsync(user, personPassword);
                    await _userManager.AddToRoleAsync(user, role.ToString());
                    user.Lastname = "User";
                    break;
            }

            await _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user));


                var person = await _personRepository.GetByUsernameAsync(email);

                var bankAccount = new BankAccount
                {
                    Currency = Currency.GEL,
                    PersonId = person!.PersonId,
                    Balance = 5000,
                    Iban = GenerateIban()
                };

                await _bankAccountRepository.AddBankAccountAsync(bankAccount);

                var cancellationToken = new CancellationToken();

                var personFullInfo = await _personRepository.GetByIdAsync(person.PersonId, cancellationToken);

                var personAccountId = personFullInfo!.BankAccounts.First().BankAccountId;


                var pinHash = _hasherService.Hash("1234");

                var encryptedCvv = EncryptionHelper.Encrypt(GenerateCvv());

                var card = new BankCard
                {
                    CardNumber = GenerateCardNumber(),
                    Cvv = encryptedCvv,
                    PinCode = pinHash,
                    ExpirationDate = DateTime.UtcNow.AddYears(5),
                    AccountId = personAccountId
                };

                await _bankCardRepository.AddCardAsync(card, cancellationToken);
        }

        
        _logger.LogSuccess("Database seeding completed successfully.");
    }

    private string GenerateIban() => $"GE{new Random().Next(100000000, 999999999)}{new Random().Next(100000000, 999999999)}";
    private string GenerateCardNumber() => $"{new Random().Next(100000000, 999999999)}{new Random().Next(10000, 99999)}";
    private string GenerateCvv() => new Random().Next(100, 999).ToString();
    private string GenerateIdNumber() => $"{new Random().Next(100000000, 999999999)}{new Random().Next(10, 99)}";
}
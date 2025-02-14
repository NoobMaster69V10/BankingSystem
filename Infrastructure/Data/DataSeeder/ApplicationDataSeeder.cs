using BankingSystem.Core.Identity;
using BankingSystem.Infrastructure.Data.DatabaseContext;
using Microsoft.AspNetCore.Identity;
using static System.Formats.Asn1.AsnWriter;

namespace BankingSystem.Infrastructure.Data.DataSeeder;

public class ApplicationDataSeeder
{
    private readonly UserManager<IdentityPerson> _userManager;
    private readonly BankingSystemDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    public ApplicationDataSeeder(BankingSystemDbContext context, UserManager<IdentityPerson> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task Seed()
    {
        await SeedData();
    }

    public async Task SeedData()
    {
        var roles = new[] { "Operator", "Person" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }


        if (!_context.IdentityPersons.Any())
        {
            var person = new IdentityPerson
            {
                UserName = "test@gmail.com",
                Email = "test@gmail.com",
                FirstName = "test",
                Lastname = "test",
                BirthDate = DateTime.Now,
                IdNumber = "02313213211"
            };

            await _userManager.CreateAsync(person, "Testtest1#");
            await _userManager.AddToRoleAsync(person, "Operator");
        }
    }
}
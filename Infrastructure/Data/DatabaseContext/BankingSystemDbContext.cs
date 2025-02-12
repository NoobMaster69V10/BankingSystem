using BankingSystem.Core.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BankingSystem.Infrastructure.Data.DatabaseContext;

public class BankingSystemDbContext : IdentityDbContext<IdentityPerson>
{
    public BankingSystemDbContext(DbContextOptions<BankingSystemDbContext> options) : base(options) { }
}

using BankingSystem.Core.Identity;

namespace BankingSystem.Infrastructure.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BankingSystemDbContext : IdentityDbContext<User>
{
    public BankingSystemDbContext(DbContextOptions<BankingSystemDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}

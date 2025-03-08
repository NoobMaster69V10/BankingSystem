using BankingSystem.Core.Identity;
using BankingSystem.Domain.Entities;
using BankingSystem.Infrastructure.Configure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BankingSystem.Infrastructure.Data.DatabaseContext;

public class BankingSystemDbContext : IdentityDbContext<IdentityPerson>
{
    public BankingSystemDbContext(DbContextOptions<BankingSystemDbContext> options) : base(options) { }

    public DbSet<IdentityPerson> IdentityPersons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new RoleConfiguration());
    }
}

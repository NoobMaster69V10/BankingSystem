using BankingSystem.Infrastructure.Configuration;
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

        builder.Entity<IdentityPerson>()
            .HasIndex(u => u.IdNumber)
            .IsUnique();
    }
}

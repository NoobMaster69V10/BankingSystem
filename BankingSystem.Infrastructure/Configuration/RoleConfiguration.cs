using BankingSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingSystem.Infrastructure.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "f9e0a56d-8a10-4cf6-9f9b-26b2c0ef7897", 
                Name = Role.Manager.ToString(),
                NormalizedName = Role.Manager.ToString().ToUpper()
            },
            new IdentityRole
            {
                Id = "e2c8d6e5-41f8-4d92-bd1a-89d7c3264913", 
                Name = Role.User.ToString(),
                NormalizedName = Role.User.ToString().ToUpper()
            },
            new IdentityRole
            {
                Id = "d6a5bfb9-3f5a-4aa2-a0a1-3d3f5a8a6311", 
                Name = Role.Operator.ToString(),
                NormalizedName = Role.Operator.ToString().ToUpper()
            }
        );
    }
}
using Microsoft.AspNetCore.Identity;

namespace BankingSystem.Core.Identity;
public class IdentityPerson : IdentityUser
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string IdNumber { get; set; }
    public DateTime BirthDate { get; set; }
}

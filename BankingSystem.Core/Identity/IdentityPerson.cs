using Microsoft.AspNetCore.Identity;

namespace BankingSystem.Core.Identity;

public class IdentityPerson : IdentityUser
{
    public string FirstName { get; set; }
    public string Lastname { get; set; }
    public string IdNumber { get; set; } 
    public override string Email { get; set; }
    
    public DateTime BirthDate { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}
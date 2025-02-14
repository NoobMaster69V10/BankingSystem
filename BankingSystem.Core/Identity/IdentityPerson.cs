using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BankingSystem.Core.Identity;
public class IdentityPerson : IdentityUser
{
    [Required(ErrorMessage = "First name is required")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Last name is required")]
    public string Lastname { get; set; }
    [Required(ErrorMessage = "IdNumber is required")]
    public string IdNumber { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public DateTime BirthDate { get; set; }
}

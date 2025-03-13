using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BankingSystem.Core.Identity;
public class IdentityPerson : IdentityUser
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50)]
    public string Lastname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Id Number is required")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")]
    public string IdNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required")]
    public override string? Email { get; set; }
    
    [Required(ErrorMessage = "Birth date is required")]
    public DateTime BirthDate { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}

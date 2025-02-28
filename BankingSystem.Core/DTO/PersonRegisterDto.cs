using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingSystem.Core.DTO;

public record PersonRegisterDto(
    [Required(ErrorMessage = "Name is required.")] string Name,
    
    [Required(ErrorMessage = "Lastname is required.")] string Lastname,
    
    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")] string IdNumber,
    
    [Required(ErrorMessage = "Birth date is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format.")] DateTime BirthDate,
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")] string Email,
    
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")] string Password
)
{
    [JsonIgnore]
    public string Role { get; set; } = "Person";
}
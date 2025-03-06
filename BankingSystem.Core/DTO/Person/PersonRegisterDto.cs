using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingSystem.Core.DTO.Person;

public record PersonRegisterDto
{
    [Required(ErrorMessage = "Name is required.")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Lastname is required.")]
    public string Lastname { get; init; } = string.Empty;

    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")]
    public string IdNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Birth date is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format.")]
    public DateTime BirthDate { get; init; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; init; } = string.Empty;
    
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [AllowedValues("Operator", "Person","Manager",ErrorMessage = "Role must be either 'Operator' or 'Person'.")]
    public string Role { get; set; } = "Person";
}
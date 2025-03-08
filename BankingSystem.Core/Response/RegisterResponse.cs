using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingSystem.Core.Response;

public record RegisterResponse
{
    [Required(ErrorMessage = "Name is required.")]
    public string FirstName { get; init; }

    [Required(ErrorMessage = "Lastname is required.")]
    public string Lastname { get; init; }

    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")]
    public string IdNumber { get; init; }

    [Required(ErrorMessage = "Birth date is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format.")]
    public DateTime BirthDate { get; init; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; init; }

    [AllowedValues("Operator", "Person", ErrorMessage = "Role must be either 'Operator' or 'Person'.")]
    [JsonIgnore]
    public string Role { get; set; } = "Person";
}
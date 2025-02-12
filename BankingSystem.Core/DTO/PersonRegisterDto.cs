using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class PersonRegisterDto
{
    public required string Name { get; set; }
    public required string Lastname { get; set; }
    public required string IdNumber { get; set; }
    public required DateTime BirthDate { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    [AllowedValues("Operator", "Person")]
    public required string Role { get; set; }
}

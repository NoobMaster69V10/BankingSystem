using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.Entities;

public class Person
{
    [Required(ErrorMessage = "Person ID is required.")]
    public required string PersonId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required.")]
    public required string Lastname { get; set; }

    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")]
    public required string IdNumber { get; set; }

    [Required(ErrorMessage = "Birth date is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format.")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; set; }

    public IList<BankAccount> BankAccounts { get; set; }
    public IList<BankCard> Cards { get; set; }
}

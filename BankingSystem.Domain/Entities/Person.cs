using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.Entities;

public class Person
{
    [Required(ErrorMessage = "Person ID is required.")]
    public string PersonId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Lastname is required.")]
    public string Lastname { get; set; }

    [Required(ErrorMessage = "ID number is required.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ID number must be exactly 11 characters.")]
    public string IdNumber { get; set; }

    [Required(ErrorMessage = "Birth date is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid birth date format.")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    public IList<BankAccount> BankAccounts { get; set; }
    public IList<BankCard> Cards { get; set; }
}

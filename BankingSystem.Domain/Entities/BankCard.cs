using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Domain.Entities;

public class BankCard
{
    public int BankCardId { get; set; }

    [Required(ErrorMessage = "Firstname is required.")]
    public  string Firstname { get; set; }

    [Required(ErrorMessage = "Lastname is required.")]
    public  string Lastname { get; set; }

    [Required(ErrorMessage = "Card number is required.")]
    [CreditCard(ErrorMessage = "Invalid card number format.")]
    public  string CardNumber { get; set; }

    [Required(ErrorMessage = "Expiration date is required.")]
    public DateTime ExpirationDate { get; set; }

    [Required(ErrorMessage = "CVV is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be exactly 3 digits.")]
    public  string Cvv { get; set; }

    [Required(ErrorMessage = "PIN code is required.")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN code must be exactly 4 digits.")]
    public string PinCode { get; set; }

    [Required(ErrorMessage = "Person ID is required.")]
    public  string PersonId { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankingSystem.Domain.Entities;

public class BankCard
{
    public int BankCardId { get; set; }

    [Required(ErrorMessage = "Firstname is required.")]
    public string? Firstname { get; set; }

    [Required(ErrorMessage = "Lastname is required.")]
    public string? Lastname { get; set; }

    [Required(ErrorMessage = "Card number is required.")]
    [CreditCard(ErrorMessage = "Invalid card number format.")]
    public string? CardNumber { get; set; }

    [Required(ErrorMessage = "Expiration date is required.")]
    public DateTime ExpirationDate { get; set; }
    public  string? Cvv { get; set; }
    public string? PinCode { get; set; }

    [Required(ErrorMessage = "Account ID is required.")]
    public int AccountId { get; set; }
}
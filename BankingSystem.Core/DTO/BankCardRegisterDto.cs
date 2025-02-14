using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public class BankCardRegisterDto
{ 
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Card number is required.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Firstname is required.")]
    public string Firstname { get; set; }

    [Required(ErrorMessage = "Lastname is required.")]
    public string Lastname { get; set; }

    [Required(ErrorMessage = "Expiration date is required.")]
    public  DateTime ExpirationDate { get; set; }

    [StringLengthFixedValidation(3, ErrorMessage = "CVV must be exactly 3 characters.")]
    public  string Cvv { get; set; }

    [StringLengthFixedValidation(4, ErrorMessage = "Pin code must be exactly 4 characters.")]
    public  string PinCode { get; set; }
}
using BankingSystem.Domain.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class CardAuthorizationDto
{
    [Required(ErrorMessage = "Card number is required")]
    public  string CardNumber { get; set; }
    [Required(ErrorMessage = "Pin Codes is required")]
    [StringLengthFixedValidation(4, ErrorMessage = "Pin code must be exactly 4 characters.")]
    public  string PinCode { get; set; }
}
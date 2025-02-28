using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public record CardAuthorizationDto(
    [Required(ErrorMessage = "Card number is required")] string CardNumber,
    
    [Required(ErrorMessage = "Pin Codes is required")]
    [StringLengthFixedValidation(4, ErrorMessage = "Pin code must be exactly 4 characters.")] string PinCode
);
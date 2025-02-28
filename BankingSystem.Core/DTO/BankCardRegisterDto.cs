using System.ComponentModel.DataAnnotations;
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public record BankCardRegisterDto(
    [Required(ErrorMessage = "Username is required.")] string Username,
    
    [Required(ErrorMessage = "Card number is required.")]
    [Length(13, 19)] string CardNumber,
    
    [Required(ErrorMessage = "Firstname is required.")] string Firstname,
    
    [Required(ErrorMessage = "Lastname is required.")] string Lastname,
    
    [Required(ErrorMessage = "Expiration date is required.")]
    [CheckExpirationDateValidation] DateTime ExpirationDate,
    
    [StringLengthFixedValidation(3, ErrorMessage = "CVV must be exactly 3 characters.")] string Cvv,
    
    [StringLengthFixedValidation(4, ErrorMessage = "Pin code must be exactly 4 characters.")] string PinCode,
    
    [Required(ErrorMessage = "BankAccountId is required.")] int BankAccountId
);

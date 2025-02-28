using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public record WithdrawMoneyDto(
    [Required(ErrorMessage = "Card number is required.")] string CardNumber,
    
    [Required(ErrorMessage = "Pin is required.")] string Pin,
    
    [Required(ErrorMessage = "Amount is required.")] int Amount,
    
    string Currency
);
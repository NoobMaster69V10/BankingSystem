using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class WithdrawMoneyDto
{
    [Required(ErrorMessage = "Card number is required.")]
    [CreditCard(ErrorMessage = "Invalid card number format.")]    
    public string CardNumber { get; set; }
    [Required(ErrorMessage = "Amount is required.")]
    public int Amount { get; set; }
    
    public string Currency { get; set; }
}
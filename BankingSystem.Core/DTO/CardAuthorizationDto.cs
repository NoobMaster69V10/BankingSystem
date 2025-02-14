using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class CardAuthorizationDto
{
    [Required(ErrorMessage = "Card number is required")]
    public  string CardNumber { get; set; }
    [Required(ErrorMessage = "Pin Codes is required")]
    public  string PinCode { get; set; }
}
using BankingSystem.Domain.CustomValidationAttributes;

namespace BankingSystem.Core.DTO;

public class BankCardRegisterDto
{
    public required string Username { get; set; }
    public required string CardNumber { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required DateTime ExpirationDate { get; set; }
    [StringLengthFixedValidation(3)]
    public required string Cvv { get; set; }
    [StringLengthFixedValidation(4)]
    public required string PinCode { get; set; }
}


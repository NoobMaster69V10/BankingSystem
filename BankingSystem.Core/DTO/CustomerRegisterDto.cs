namespace BankingSystem.Core.DTO;

public class CustomerRegisterDto
{
    public string Name { get; set; }
    public string Lastname { get; set; }
    public string IdNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}

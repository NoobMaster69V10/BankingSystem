using System.ComponentModel.DataAnnotations;

namespace BankingSystem.Core.DTO;

public class TransactionCsvDto
{
    [Required] 
    public string PersonId { get; set; }
    public DateTime startDate {get; set;}
    public DateTime endDate { get; set; }
}
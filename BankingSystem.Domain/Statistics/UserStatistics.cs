namespace BankingSystem.Domain.Statistics;

public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int RegisteredThisYear { get; set; }
    public int RegisteredLastYear { get; set; }
    public int RegisteredLast30Days { get; set; }
}
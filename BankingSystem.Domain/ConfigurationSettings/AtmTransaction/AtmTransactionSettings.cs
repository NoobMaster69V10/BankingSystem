namespace BankingSystem.Domain.ConfigurationSettings.AtmTransaction;

public class AtmTransactionSettings
{
    public int DailyLimit { get; set; }
    public decimal WithdrawalFee { get; set; }
}
namespace BankingSystem.Core.ServiceContracts;

public interface IHasherService
{
    string Hash(string input);
    bool Verify(string input, string hash);
}
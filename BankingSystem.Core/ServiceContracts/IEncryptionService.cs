namespace BankingSystem.Core.ServiceContracts;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
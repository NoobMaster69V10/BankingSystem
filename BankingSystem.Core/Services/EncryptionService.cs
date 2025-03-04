using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.ConfigurationSettings.Encryption;

namespace BankingSystem.Core.Services;

public class EncryptionService : IEncryptionService
{
    private string EncryptionKey { get; }
    public EncryptionService(IOptions<EncryptionSettings> encryptionOptions)
    {
        EncryptionKey = encryptionOptions.Value.EncryptionKey;
    }
    public string Encrypt(string plainText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));
            aes.IV = new byte[16];

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);
        return reader.ReadToEnd();
    }
}
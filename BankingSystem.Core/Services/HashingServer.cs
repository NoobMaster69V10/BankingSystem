using System.Security.Cryptography;
using BankingSystem.Core.Helpers;
using BankingSystem.Core.ServiceContracts;

namespace BankingSystem.Core.Services;

public sealed class HashingServer : IHasherService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public  string Hash(string input)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string input, string storedHash)
    {
        var parts = storedHash.Split('-');
        var hash = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, Algorithm, HashSize); 
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
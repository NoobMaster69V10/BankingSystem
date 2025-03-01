using System.Security.Cryptography;

namespace BankingSystem.Core.Helpers;

public static class HashingHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

    public static (string hashedPin, string salt) HashPinAndCvv(string pin)
    {
        var saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        var salt = Convert.ToBase64String(saltBytes);

        var pbkdf2Pin = new Rfc2898DeriveBytes(pin, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var hashedPin = Convert.ToBase64String(pbkdf2Pin.GetBytes(KeySize));

        return (hashedPin, salt);
    }

    public static bool VerifyHash(string input, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var newHash = Convert.ToBase64String(pbkdf2.GetBytes(KeySize));

        return newHash == storedHash;
    }
}
using System.Security.Cryptography;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Identity;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2-SHA256";
    private const int Iterations = 100_000;
    private const int SaltSizeBytes = 16; // 128-bit
    private const int KeySizeBytes = 32; // 256-bit
    private static readonly HashAlgorithmName Sha256 = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        if (password is null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        byte[] hash = Derive(password, salt, Iterations);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (password is null || storedHash is null)
        {
            return false;
        }

        try
        {
            string[] parts = storedHash.Split('$');
            if (parts.Length != 4)
            {
                return false;
            }

            if (!string.Equals(parts[0], Prefix, StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out int iterations) || iterations < Iterations)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length != SaltSizeBytes || expectedHash.Length != KeySizeBytes)
            {
                return false;
            }

            byte[] actualHash = Derive(password, salt, iterations);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] Derive(string password, byte[] salt, int iterations)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, Sha256);
        return pbkdf2.GetBytes(KeySizeBytes);
    }
}

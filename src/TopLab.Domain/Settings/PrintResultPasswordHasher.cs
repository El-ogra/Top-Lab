using System.Security.Cryptography;
using System.Text;

namespace TopLab.Domain.Settings;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.2): hashes and verifies the print-result
/// password. The plain password is never persisted — only the value produced
/// by <see cref="Hash"/> is stored, and it is the only form
/// <see cref="Verify"/> accepts.
/// </summary>
/// <remarks>
/// Format: <c>pbkdf2$&lt;iterations&gt;$&lt;base64 salt&gt;$&lt;base64 subkey&gt;</c>.
/// PBKDF2-HMAC-SHA256, a fresh 16-byte random salt per hash and a 32-byte
/// derived key. The stored string stays far below the 300-character column.
/// </remarks>
public static class PrintResultPasswordHasher
{
    public const int MaxHashLength = 300;

    private const int SaltSize = 16;
    private const int SubKeySize = 32;
    private const int DefaultIterations = 100_000;
    private const string Prefix = "pbkdf2";

    /// <summary>Produces the stored form of a plain password.</summary>
    public static string Hash(string password, int iterations = DefaultIterations)
    {
        if (password is null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        if (iterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iterations));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var subKey = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            SubKeySize);

        return $"{Prefix}${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subKey)}";
    }

    /// <summary>
    /// Verifies a plain password against a stored hash. A missing or malformed
    /// hash never matches.
    /// </summary>
    public static bool Verify(string? password, string? storedHash)
    {
        if (password is null || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !string.Equals(parts[0], Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

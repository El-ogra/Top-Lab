using TopLab.Infrastructure.Identity;

namespace TopLab.Infrastructure.Tests.Identity;

public class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Hash_ThenVerify_OnFreshHasher_Succeeds()
    {
        var hasher1 = new Pbkdf2PasswordHasher();
        var hasher2 = new Pbkdf2PasswordHasher();

        string hash = hasher1.Hash("P@ssw0rd123");

        Assert.True(hasher2.Verify("P@ssw0rd123", hash));
    }

    [Fact]
    public void Verify_WrongPassword_Fails()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("correct-password");

        Assert.False(hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_TwoHashesOfSamePassword_DifferButBothVerify()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash1 = hasher.Hash("same-password");
        string hash2 = hasher.Hash("same-password");

        Assert.NotEqual(hash1, hash2);
        Assert.True(hasher.Verify("same-password", hash1));
        Assert.True(hasher.Verify("same-password", hash2));
    }

    [Fact]
    public void Verify_TamperedStoredHash_Fails()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("password123");

        // Tamper: flip last char
        char last = hash[^1];
        char flipped = last == 'A' ? 'B' : 'A';
        string tampered = hash.Substring(0, hash.Length - 1) + flipped;

        Assert.False(hasher.Verify("password123", tampered));
    }

    [Fact]
    public void Verify_MalformedStoredHash_ReturnsFalseWithoutThrowing()
    {
        var hasher = new Pbkdf2PasswordHasher();

        Assert.False(hasher.Verify("password", "not-a-valid-hash"));
        Assert.False(hasher.Verify("password", "PBKDF2-SHA256$100000$only-two-parts"));
        Assert.False(hasher.Verify("password", ""));
        Assert.False(hasher.Verify("password", "PBKDF2-SHA256$not-a-number$salt$hash"));
        Assert.False(hasher.Verify("password", "WRONG-PREFIX$100000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="));
    }

    [Fact]
    public void Hash_OutputLength_NeverExceeds300Characters()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("any-password-value");

        Assert.True(hash.Length <= 300);
        // Also verify it fits comfortably: expected ~90-120
        Assert.True(hash.Length >= 50);
    }

    [Fact]
    public void Hash_EmbeddedIterationCount_IsAtLeast100000()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("password123");

        string[] parts = hash.Split('$');
        Assert.Equal(4, parts.Length);
        Assert.Equal("PBKDF2-SHA256", parts[0]);
        Assert.True(int.TryParse(parts[1], out int iterations));
        Assert.True(iterations >= 100_000);
    }
}

using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    private readonly Dictionary<string, string> _hashToPassword = new();
    private readonly Dictionary<string, string> _passwordToHash = new();

    public string Hash(string password)
    {
        if (_passwordToHash.TryGetValue(password, out var existing))
        {
            return existing;
        }

        string hash = $"HASH:{password}";
        _passwordToHash[password] = hash;
        _hashToPassword[hash] = password;
        return hash;
    }

    public bool Verify(string password, string storedHash)
    {
        if (storedHash is null || password is null) return false;
        if (_hashToPassword.TryGetValue(storedHash, out var original))
        {
            return original == password;
        }

        // Fallback: support real hasher format for tests that use real hasher strings?
        return storedHash == $"HASH:{password}";
    }

    public void Seed(string password, string hash)
    {
        _passwordToHash[password] = hash;
        _hashToPassword[hash] = password;
    }
}

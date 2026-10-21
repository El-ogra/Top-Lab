using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.2 / 1.2.2.4.2.1): the print-result password is
/// stored only as a hash, and the plain value never leaves the call.
/// </summary>
public class PrintResultPasswordHasherTests
{
    [Fact]
    public void Hash_DoesNotContainThePlainPassword()
    {
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.DoesNotContain("s3cret!", hash, StringComparison.Ordinal);
        Assert.StartsWith("pbkdf2$", hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Hash_FitsTheColumnLength()
    {
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.True(hash.Length <= PrintResultPasswordHasher.MaxHashLength);
    }

    [Fact]
    public void Verify_CorrectPassword_True()
    {
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.True(PrintResultPasswordHasher.Verify("s3cret!", hash));
    }

    [Fact]
    public void Verify_WrongPassword_False()
    {
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.False(PrintResultPasswordHasher.Verify("other", hash));
    }

    [Fact]
    public void Verify_EmptyPassword_False()
    {
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.False(PrintResultPasswordHasher.Verify(string.Empty, hash));
    }

    [Fact]
    public void Verify_NullPasswordOrHash_False()
    {
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.False(PrintResultPasswordHasher.Verify(null, hash));
        Assert.False(PrintResultPasswordHasher.Verify("s3cret!", null));
    }

    [Fact]
    public void Verify_MalformedHash_False()
    {
        Assert.False(PrintResultPasswordHasher.Verify("s3cret!", "not-a-hash"));
        Assert.False(PrintResultPasswordHasher.Verify("s3cret!", "pbkdf2$notanumber$c2FsdA==$c2FsdA=="));
        Assert.False(PrintResultPasswordHasher.Verify("s3cret!", "pbkdf2$1000$notbase64$notbase64"));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        var first = PrintResultPasswordHasher.Hash("s3cret!");
        var second = PrintResultPasswordHasher.Hash("s3cret!");

        Assert.NotEqual(first, second);
        Assert.True(PrintResultPasswordHasher.Verify("s3cret!", first));
        Assert.True(PrintResultPasswordHasher.Verify("s3cret!", second));
    }
}

/// <summary>STAGE-01: the print-result password requirement on the settings row.</summary>
public class SystemSettingsPrintResultPasswordTests
{
    [Fact]
    public void CreateDefault_HasTheRequirementDisabled()
    {
        var settings = SystemSettings.CreateDefault();

        Assert.False(settings.PrintResultPasswordEnabled);
        Assert.Null(settings.PrintResultPasswordHash);
    }

    [Fact]
    public void SetPrintResultPassword_StoresTheHashAndTheFlag()
    {
        var settings = SystemSettings.CreateDefault();
        var hash = PrintResultPasswordHasher.Hash("s3cret!");

        settings.SetPrintResultPassword(true, hash);

        Assert.True(settings.PrintResultPasswordEnabled);
        Assert.Equal(hash, settings.PrintResultPasswordHash);
    }

    [Fact]
    public void SetPrintResultPassword_Disabling_ClearsTheHash()
    {
        var settings = SystemSettings.CreateDefault();
        settings.SetPrintResultPassword(true, PrintResultPasswordHasher.Hash("s3cret!"));

        settings.SetPrintResultPassword(false, null);

        Assert.False(settings.PrintResultPasswordEnabled);
        Assert.Null(settings.PrintResultPasswordHash);
    }

    [Fact]
    public void SetPrintResultPassword_TooLongHash_Throws()
    {
        var settings = SystemSettings.CreateDefault();

        Assert.Throws<ArgumentException>(() =>
            settings.SetPrintResultPassword(true, new string('h', PrintResultPasswordHasher.MaxHashLength + 1)));
    }
}

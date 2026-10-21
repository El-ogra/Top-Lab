using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings.Common;

/// <summary>
/// STAGE-01 (baseline 1.2.2.4.2): the shared verifier that the print entry
/// points of STAGE-06, STAGE-08 and STAGE-10 call. STAGE-01 delivers the
/// verifier; the enforcement at the print entry points is those stages' work.
/// </summary>
public class PrintResultPasswordVerifierTests
{
    private static (FakeApplicationDbContext Db, PrintResultPasswordVerifier Verifier) Build(bool enabled, string? password)
    {
        var db = new FakeApplicationDbContext();
        var settings = SystemSettings.CreateDefault();
        settings.SetPrintResultPassword(enabled, enabled ? PrintResultPasswordHasher.Hash(password!) : null);
        db.SystemSettings.Add(settings);

        return (db, new PrintResultPasswordVerifier(db));
    }

    [Fact]
    public void IsRequired_FalseWhenTheRequirementIsOff()
    {
        var (_, verifier) = Build(false, null);

        Assert.False(verifier.IsRequired());
    }

    [Fact]
    public void IsRequired_TrueWhenTheRequirementIsOn()
    {
        var (_, verifier) = Build(true, "s3cret!");

        Assert.True(verifier.IsRequired());
    }

    [Fact]
    public void Verify_AcceptsTheCorrectPassword()
    {
        var (_, verifier) = Build(true, "s3cret!");

        Assert.True(verifier.Verify("s3cret!"));
    }

    [Fact]
    public void Verify_RefusesAWrongOrMissingPassword()
    {
        var (_, verifier) = Build(true, "s3cret!");

        Assert.False(verifier.Verify("wrong"));
        Assert.False(verifier.Verify(null));
        Assert.False(verifier.Verify(string.Empty));
    }

    [Fact]
    public void Verify_AcceptsAnythingWhenTheRequirementIsOff()
    {
        var (_, verifier) = Build(false, null);

        Assert.True(verifier.Verify(null));
        Assert.True(verifier.Verify("anything"));
    }

    [Fact]
    public void Verify_AcceptsAnythingWhenTheSettingsRowIsMissing()
    {
        var db = new FakeApplicationDbContext();
        var verifier = new PrintResultPasswordVerifier(db);

        Assert.False(verifier.IsRequired());
        Assert.True(verifier.Verify(null));
    }
}

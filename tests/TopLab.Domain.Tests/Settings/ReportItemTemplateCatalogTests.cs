using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class ReportItemTemplateCatalogTests
{
    [Fact]
    public void Defaults_HasOneRowPerCanonicalKey()
    {
        Assert.Equal(ReportItemKeys.All.Count, ReportItemTemplateCatalog.Defaults.Count);
    }

    [Fact]
    public void Defaults_KeysMatchCanonicalKeysExactly()
    {
        var defaultKeys = ReportItemTemplateCatalog.Defaults.Select(d => d.ItemKey).ToHashSet(StringComparer.Ordinal);
        var canonical = ReportItemKeys.All.ToHashSet(StringComparer.Ordinal);

        Assert.True(canonical.SetEquals(defaultKeys));
    }

    [Fact]
    public void Defaults_KeysAreDistinct()
    {
        var keys = ReportItemTemplateCatalog.Defaults.Select(d => d.ItemKey).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Defaults_LabelsAreNonBlankAndVisible()
    {
        Assert.All(ReportItemTemplateCatalog.Defaults, d =>
        {
            Assert.False(string.IsNullOrWhiteSpace(d.Label));
            Assert.True(d.ShowHide);
            Assert.False(d.FreeTextOverride);
        });
    }

    [Fact]
    public void GetDefault_KnownKey_ReturnsSameLabelAsDefaults()
    {
        foreach (var key in ReportItemKeys.All)
        {
            var fromCatalog = ReportItemTemplateCatalog.Defaults.Single(d => d.ItemKey == key);
            Assert.Equal(fromCatalog.Label, ReportItemTemplateCatalog.GetDefault(key).Label);
        }
    }

    [Fact]
    public void GetDefault_UnknownKey_ReturnsVisiblePermissiveFallback()
    {
        var fallback = ReportItemTemplateCatalog.GetDefault("NotAKey");

        Assert.Equal("NotAKey", fallback.ItemKey);
        Assert.Equal("NotAKey", fallback.Label);
        Assert.True(fallback.ShowHide);
        Assert.False(fallback.FreeTextOverride);
    }

    [Fact]
    public void GetDefault_BlankKey_ReturnsDashFallback()
    {
        var fallback = ReportItemTemplateCatalog.GetDefault("   ");
        Assert.Equal("-", fallback.Label);
    }
}

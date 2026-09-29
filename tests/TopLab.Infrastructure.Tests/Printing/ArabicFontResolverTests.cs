using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// FIX-N2: ArabicFontResolver must always return a non-empty family and never throw,
/// including when the requested family is null/empty/unknown. Used by Receipt/Invoice/WorkSheet
/// PDF writers in place of a hard-coded "Arial" fallback.
/// </summary>
public class ArabicFontResolverTests
{
#pragma warning disable CA1416 // Windows-only font APIs inside ArabicFontResolver

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_NullOrEmpty_ReturnsNonEmptyFallbackWithoutThrowing(string? family)
    {
        var resolved = ArabicFontResolver.Resolve(family);

        Assert.False(string.IsNullOrWhiteSpace(resolved));
    }

    [Fact]
    public void Resolve_Arial_ReturnsNonEmpty()
    {
        var resolved = ArabicFontResolver.Resolve("Arial");

        Assert.False(string.IsNullOrWhiteSpace(resolved));
    }

    [Fact]
    public void Resolve_UnknownFamily_ReturnsNonEmpty()
    {
        var resolved = ArabicFontResolver.Resolve("NoSuchFontFamily-XYZ-123");

        Assert.False(string.IsNullOrWhiteSpace(resolved));
    }

    [Fact]
    public void Resolve_PreferredOrRequestedFamily_IsNeverBlank_ForTypicalInputs()
    {
        // Behavioural contract shared by Receipt/Invoice/WorkSheet writers:
        // whatever the lab settings say, a family name comes back for QuestPDF.
        foreach (var family in new[] { null, "", "Arial", "Segoe UI", "NoSuchFontFamily-XYZ-123" })
        {
            var resolved = ArabicFontResolver.Resolve(family);
            Assert.False(string.IsNullOrWhiteSpace(resolved), $"Resolve({family ?? "null"}) returned blank");
        }
    }

#pragma warning restore CA1416
}
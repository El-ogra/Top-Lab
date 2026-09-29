using System.IO;

namespace TopLab.Presentation.Tests;

/// <summary>Owner decision: About window must not show Placeholder stubs.</summary>
public sealed class AboutWindowPlaceholderTests
{
    private static string AboutXamlPath =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "TopLab.Presentation", "Views", "Shell", "AboutWindow.xaml"));

    [Fact]
    public void AboutWindow_HasNoPlaceholderText()
    {
        var path = AboutXamlPath;
        Assert.True(File.Exists(path), $"AboutWindow.xaml not found at {path}");
        var content = File.ReadAllText(path);
        Assert.DoesNotContain("Placeholder", content);
    }

    [Fact]
    public void AboutWindow_KeepsRealVersionLine()
    {
        var path = AboutXamlPath;
        var content = File.ReadAllText(path);
        Assert.Contains("VersionText", content);
    }
}

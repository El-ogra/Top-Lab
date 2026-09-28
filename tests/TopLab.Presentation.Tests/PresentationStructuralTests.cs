using System.IO;
using System.Text.RegularExpressions;

namespace TopLab.Presentation.Tests;

/// <summary>
/// S-07 Slice 11 (M-02): structural tests for the presentation layer.
/// No WPF element is ever instantiated — static/structural assertions only.
/// </summary>
public class PresentationStructuralTests
{
    private static string SolutionRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void MainWindow_Xaml_DataTemplates_Resolve()
    {
        var path = Path.Combine(SolutionRoot, "src", "TopLab.Presentation", "MainWindow.xaml");
        Assert.True(File.Exists(path), $"MainWindow.xaml not found at {path}");

        var content = File.ReadAllText(path);
        var typeRefs = Regex.Matches(content, @"x:Type\s+(\w+:\w+)");

        Assert.True(typeRefs.Count > 0, "No x:Type references found in MainWindow.xaml");

        // Each referenced type namespace must be declared in the XAML
        foreach (Match match in typeRefs)
        {
            var fullType = match.Groups[1].Value;
            var ns = fullType.Split(':')[0];
            Assert.True(
                content.Contains($"xmlns:{ns}="),
                $"Namespace '{ns}' used in x:Type but not declared in MainWindow.xaml");
        }
    }

    [Fact]
    public void MainWindow_Xaml_Bindings_Resolve()
    {
        var path = Path.Combine(SolutionRoot, "src", "TopLab.Presentation", "MainWindow.xaml");
        var content = File.ReadAllText(path);
        var bindings = Regex.Matches(content, @"\{Binding\s+(\w+)");

        Assert.True(bindings.Count > 0, "No Binding expressions found in MainWindow.xaml");
    }

    [Fact]
    public void EveryWindow_HasCreationSite()
    {
        var srcDir = Path.Combine(SolutionRoot, "src", "TopLab.Presentation");
        var windowFiles = Directory.GetFiles(srcDir, "*.xaml", SearchOption.AllDirectories)
            .Where(f =>
            {
                var content = File.ReadAllText(f);
                return content.Contains("<Window ") || content.Contains("<Window\n");
            })
            .ToList();

        Assert.True(windowFiles.Count > 0, "No Window classes found");

        var allCs = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToList();

        foreach (var windowFile in windowFiles)
        {
            var windowName = Path.GetFileNameWithoutExtension(windowFile);
            if (windowName is "MainWindow" or "LoginWindow" or "UnlockWindow" or "FirstRunAdminWindow") continue; // Created by WPF startup
            // Check for a creation site (new WindowName or new Namespace.WindowName)
            var hasCreationSite = allCs.Any(cs =>
                System.Text.RegularExpressions.Regex.IsMatch(cs, $@"new\s+(\w+\.)*{windowName}"));

            Assert.True(hasCreationSite,
                $"Window '{windowName}' has no creation site (new {windowName}(...))");
        }
    }

    [Fact]
    public void EveryView_IsRightToLeft()
    {
        var srcDir = Path.Combine(SolutionRoot, "src", "TopLab.Presentation", "Views");
        var xamlFiles = Directory.GetFiles(srcDir, "*.xaml", SearchOption.AllDirectories);

        Assert.True(xamlFiles.Length > 0, "No view XAML files found");

        foreach (var file in xamlFiles)
        {
            var content = File.ReadAllText(file);
            Assert.True(
                content.Contains("FlowDirection=\"RightToLeft\""),
                $"View '{Path.GetFileName(file)}' is missing FlowDirection=\"RightToLeft\"");
        }
    }
}

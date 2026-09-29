using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TopLab.Presentation.ViewModels.Shell;

namespace TopLab.Presentation.Tests;

/// <summary>
/// S-07 Slice 11 (M-02): structural tests for the presentation layer.
/// Resolves x:Type and Binding names against real CLR types/properties — not
/// xmlns strings or substring presence. No WPF element is instantiated.
/// </summary>
public class PresentationStructuralTests
{
    private static string SolutionRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string MainWindowPath =>
        Path.Combine(SolutionRoot, "src", "TopLab.Presentation", "MainWindow.xaml");

    private static Assembly PresentationAssembly => typeof(ShellViewModel).Assembly;

    private static Dictionary<string, string> ParseXmlns(XDocument doc)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (doc.Root is null)
        {
            return map;
        }

        foreach (var attr in doc.Root.Attributes())
        {
            if (attr.Name.Namespace == XNamespace.Xmlns)
            {
                map[attr.Name.LocalName] = attr.Value;
            }
        }

        return map;
    }

    /// <summary>Resolves clr-namespace URI to a CLR namespace name.</summary>
    private static string? ClrNamespaceFromXmlns(string xmlnsValue)
    {
        const string prefix = "clr-namespace:";
        if (!xmlnsValue.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var rest = xmlnsValue.Substring(prefix.Length);
        var semi = rest.IndexOf(';');
        return semi >= 0 ? rest.Substring(0, semi) : rest;
    }

    [Fact]
    public void MainWindow_Xaml_DataTemplates_ResolveToClrTypes()
    {
        Assert.True(File.Exists(MainWindowPath), $"MainWindow.xaml not found at {MainWindowPath}");

        var doc = XDocument.Load(MainWindowPath);
        var xmlns = ParseXmlns(doc);
        var typeRefs = Regex.Matches(File.ReadAllText(MainWindowPath), @"x:Type\s+(\w+):([\w.]+)");

        Assert.True(typeRefs.Count > 0, "No x:Type references found in MainWindow.xaml");

        foreach (Match match in typeRefs)
        {
            var prefix = match.Groups[1].Value;
            var typeName = match.Groups[2].Value;

            Assert.True(xmlns.TryGetValue(prefix, out var nsUri),
                $"x:Type prefix '{prefix}' is not declared as xmlns:{prefix} in MainWindow.xaml");

            var clrNs = ClrNamespaceFromXmlns(nsUri!)
                ?? throw new Xunit.Sdk.XunitException(
                    $"xmlns:{prefix}='{nsUri}' is not a clr-namespace mapping.");

            var fullTypeName = $"{clrNs}.{typeName}";
            var type = PresentationAssembly.GetType(fullTypeName)
                ?? Type.GetType($"{fullTypeName}, {PresentationAssembly.GetName().Name}");

            Assert.True(type is not null,
                $"x:Type {prefix}:{typeName} does not resolve to a CLR type '{fullTypeName}' in the Presentation assembly.");
        }
    }

    [Fact]
    public void MainWindow_Xaml_Bindings_ResolveToPublicProperties()
    {
        Assert.True(File.Exists(MainWindowPath), $"MainWindow.xaml not found at {MainWindowPath}");

        var content = File.ReadAllText(MainWindowPath);
        var bindings = Regex.Matches(content, @"\{Binding\s+(\w+)");

        Assert.True(bindings.Count > 0, "No Binding expressions found in MainWindow.xaml");

        // MainWindow DataContext is ShellViewModel; ItemTemplate rows bind NavigationItem.
        var shellProps = typeof(ShellViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);
        var navItemProps = typeof(NavigationItem).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (Match match in bindings)
        {
            var prop = match.Groups[1].Value;
            Assert.True(
                shellProps.Contains(prop) || navItemProps.Contains(prop),
                $"Binding '{prop}' does not exist as a public property on {nameof(ShellViewModel)} or {nameof(NavigationItem)}.");
        }
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

        // Host-created windows (App composition root / WPF startup). Each has an explicit reason.
        var frameworkCreated = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Startup shell — resolved from DI in App.OnStartup (`GetRequiredService<MainWindow>()`).
            ["MainWindow"] = "DI-resolved host window in App.OnStartup",
            // Login gate — resolved from DI in App.OnStartup (`GetRequiredService<LoginWindow>()`).
            ["LoginWindow"] = "DI-resolved host window in App.OnStartup",
        };

        foreach (var windowFile in windowFiles)
        {
            var windowName = Path.GetFileNameWithoutExtension(windowFile);

            if (frameworkCreated.ContainsKey(windowName))
            {
                // Prove the DI creation site exists (not just a name exclusion).
                var hasDiSite = allCs.Any(cs =>
                    Regex.IsMatch(cs, $@"GetRequiredService\s*<\s*(\w+\.)*{windowName}\s*>"));
                Assert.True(hasDiSite,
                    $"Excluded window '{windowName}' is claimed framework-created but has no GetRequiredService<{windowName}> site. Reason: {frameworkCreated[windowName]}");
                continue;
            }

            var hasNewSite = allCs.Any(cs =>
                Regex.IsMatch(cs, $@"new\s+(\w+\.)*{windowName}"));
            var hasDiSiteOther = allCs.Any(cs =>
                Regex.IsMatch(cs, $@"GetRequiredService\s*<\s*(\w+\.)*{windowName}\s*>"));

            Assert.True(hasNewSite || hasDiSiteOther,
                $"Window '{windowName}' has no creation site (new {windowName}(...) or GetRequiredService<{windowName}>).");
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
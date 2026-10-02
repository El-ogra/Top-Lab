using System.IO;

namespace TopLab.Presentation.Tests.Layering;

/// <summary>W-02 S15 (WP-29, SD-12/C-18): no Presentation type may reference
/// Infrastructure — except the composition root. The three existing violations
/// are recorded debt (numbered TODOs), not refactored in this wave.</summary>
public class PresentationLayeringTests
{
    private static string PresentationRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "TopLab.Presentation");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate TopLab.Presentation sources.");
    }

    [Fact]
    public void PresentationLayering_NoInfrastructureReferenceOutsideAppXaml()
    {
        var offenders = Directory
            .EnumerateFiles(PresentationRoot(), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("App.xaml.cs", StringComparison.Ordinal))
            .Where(f => File.ReadAllText(f).Contains("TopLab.Infrastructure", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .OrderBy(n => n)
            .ToList();

        // TODO-W02-1 CombinedReportViewModel.cs — preview path (accepted debt).
        // TODO-W02-2 ProfileEntryViewModel.cs — preview path (accepted debt).
        // TODO-W02-3 WorkSheetsViewModel.cs — direct writer construction (accepted debt).
        Assert.Equal(
            new[] { "CombinedReportViewModel.cs", "ProfileEntryViewModel.cs", "WorkSheetsViewModel.cs" },
            offenders);
    }

    [Fact]
    public void WorkSheetsViewModel_StillBypassesTheRegisteredWriter()
    {
        var path = Path.Combine(
            PresentationRoot(), "ViewModels", "WorkSheets", "WorkSheetsViewModel.cs");
        var source = File.ReadAllText(path);

        Assert.Contains("new WorkSheetPdfWriter()", source, StringComparison.Ordinal);
    }
}

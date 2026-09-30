using Xunit;

namespace TopLab.Persistence.Tests.Migrations;

/// <summary>
/// WP-03 repair map (SD-1) — pure CASE logic, no database required.
/// 0→NULL · 1→0 · 2→1 · 3→2
/// </summary>
public class SensitivityCategoryRepairTests
{
    private static int? Repair(int stored) => stored switch
    {
        0 => null,
        1 => 0,
        2 => 1,
        3 => 2,
        _ => stored
    };

    [Fact]
    public void SensitivityRepair_MapsStoredZero_ToNull()
    {
        Assert.Null(Repair(0));
    }

    [Fact]
    public void SensitivityRepair_MapsStoredOne_ToZero()
    {
        Assert.Equal(0, Repair(1));
    }

    [Fact]
    public void SensitivityRepair_MapsStoredTwo_ToOne()
    {
        Assert.Equal(1, Repair(2));
    }

    [Fact]
    public void SensitivityRepair_MapsStoredThree_ToTwo()
    {
        Assert.Equal(2, Repair(3));
    }

    [Fact]
    public void SensitivityRepair_MigrationFile_ContainsBackupAndCaseMap()
    {
        var root = FindRepoRoot();
        var dir = Path.Combine(root, "src", "TopLab.Infrastructure", "Persistence", "Migrations");
        var file = Directory.EnumerateFiles(dir, "*FixCultureSensitivityCategoryOffByOne.cs")
            .First(f => !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase));
        var text = File.ReadAllText(file);

        Assert.Contains("CultureSensitivityBackup", text, StringComparison.Ordinal);
        Assert.Contains("WHEN 0 THEN NULL", text, StringComparison.Ordinal);
        Assert.Contains("WHEN 1 THEN 0", text, StringComparison.Ordinal);
        Assert.Contains("WHEN 2 THEN 1", text, StringComparison.Ordinal);
        Assert.Contains("WHEN 3 THEN 2", text, StringComparison.Ordinal);
    }

    [Fact]
    public void SensitivityRepair_DoesNotTouchExistingMigrations()
    {
        var root = FindRepoRoot();
        var dir = Path.Combine(root, "src", "TopLab.Infrastructure", "Persistence", "Migrations");
        var existingSuffixes = new[]
        {
            "_BaselineDataModel.cs",
            "_RenamePkColumns.cs",
            "_AddTestCodeAndLifecycleColumns.cs",
            "_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs",
            "_AddPatientTestReferenceRangeSnapshots.cs",
            "_AddAnalyteProfileDomain.cs",
            "_AddPregnancyMedicalConditionTypeSeed.cs",
            "_AddInvoiceIssues.cs"
        };

        var files = Directory.EnumerateFiles(dir, "*.cs")
            .Where(f => !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .ToList();

        foreach (var suffix in existingSuffixes)
        {
            Assert.Contains(files, f => f!.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TopLab.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate TopLab.sln");
    }
}

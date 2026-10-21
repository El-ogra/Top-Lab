using Xunit;

namespace TopLab.Persistence.Tests.Migrations;

/// <summary>
/// STAGE-01: the structure of the migration that adds the card number, the
/// ultrasound flag, the print-result password columns and the eleven
/// medical-history catalogue rows.
/// </summary>
/// <remarks>
/// These assertions read the migration artefacts as text. They never connect
/// to a database and never apply a migration.
/// </remarks>
public class AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogueTests
{
    private const string MigrationSuffix = "_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.cs";
    private const string DesignerSuffix = "_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.Designer.cs";

    private static string MigrationsDir => Path.Combine(
        FindRepoRoot(), "src", "TopLab.Infrastructure", "Persistence", "Migrations");

    private static string MigrationFile
    {
        get
        {
            var file = Directory.EnumerateFiles(MigrationsDir, "*" + MigrationSuffix)
                .FirstOrDefault(f => !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(file);
            return file!;
        }
    }

    private static string MigrationText => File.ReadAllText(MigrationFile);

    private static string SnapshotText => File.ReadAllText(
        Path.Combine(MigrationsDir, "ApplicationDbContextModelSnapshot.cs"));

    [Fact]
    public void TheMigrationAndItsDesignerExist()
    {
        Assert.NotEmpty(MigrationFile);

        var designer = Directory.EnumerateFiles(MigrationsDir, "*" + DesignerSuffix).ToList();
        Assert.Single(designer);
        Assert.Contains("AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue", File.ReadAllText(designer[0]));
    }

    [Fact]
    public void TheMigrationAddsTheFourColumnsWithTheirDefaults()
    {
        var text = MigrationText;

        Assert.Contains("name: \"CardNumber\"", text, StringComparison.Ordinal);
        Assert.Contains("name: \"RecentUltrasoundImaging\"", text, StringComparison.Ordinal);
        Assert.Contains("name: \"PrintResultPasswordEnabled\"", text, StringComparison.Ordinal);
        Assert.Contains("name: \"PrintResultPasswordHash\"", text, StringComparison.Ordinal);

        Assert.Contains("type: \"nvarchar(30)\"", text, StringComparison.Ordinal);
        Assert.Contains("maxLength: 30", text, StringComparison.Ordinal);
        Assert.Contains("type: \"nvarchar(300)\"", text, StringComparison.Ordinal);
        Assert.Contains("maxLength: 300", text, StringComparison.Ordinal);

        // Both boolean columns are NOT NULL with a behaviour-preserving default.
        Assert.Equal(2, CountOccurrences(text, "defaultValue: false"));
    }

    [Fact]
    public void TheMigrationIndexesTheCardNumber()
    {
        var text = MigrationText;

        Assert.Contains("migrationBuilder.CreateIndex(", text, StringComparison.Ordinal);
        Assert.Contains("name: \"IX_Patients_CardNumber\"", text, StringComparison.Ordinal);
        Assert.Contains("table: \"Patients\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMigrationSeedsTheElevenCatalogueRows()
    {
        var text = MigrationText;

        Assert.Contains("table: \"MedicalConditionTypes\"", text, StringComparison.Ordinal);

        var expected = new (int Id, byte Category, string Name)[]
        {
            (2, 0, "هل يؤخذ علاج للسكر"),
            (3, 0, "هل يؤخذ علاج لسيولة الدم"),
            (4, 0, "هل يؤخذ علاج للغدد"),
            (5, 0, "هل يؤخذ علاج للفيروسات"),
            (6, 0, "هل يؤخذ مضاد حيوي"),
            (7, 0, "هل يؤخذ علاج للكبد"),
            (8, 1, "مريض بفقر الدم"),
            (9, 1, "مريض بالتهاب المفاصل"),
            (10, 1, "مريض بالضغط الدم"),
            (11, 1, "مريض بالفشل كلوي"),
            (12, 1, "مريض بالذئبة الحمراء")
        };

        foreach (var (id, category, name) in expected)
        {
            Assert.Contains($"{{ {id}, (byte){category}, \"{name}\" }}", text, StringComparison.Ordinal);
        }

        // The pregnancy row is never re-inserted: it belongs to the earlier migration.
        Assert.DoesNotContain("{ 1, (byte)2,", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMigrationUpdatesTheSeededSettingsRow()
    {
        var text = MigrationText;

        Assert.Contains("migrationBuilder.UpdateData(", text, StringComparison.Ordinal);
        Assert.Contains("keyColumn: \"SystemSettingsId\"", text, StringComparison.Ordinal);
        Assert.Contains("keyValue: 1", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRollbackDropsTheColumnsTheIndexAndTheSeededRows()
    {
        var text = MigrationText;

        Assert.Contains("migrationBuilder.DropColumn(", text, StringComparison.Ordinal);
        Assert.Equal(4, CountOccurrences(text, "migrationBuilder.DropColumn("));
        Assert.Contains("migrationBuilder.DropIndex(", text, StringComparison.Ordinal);

        // The eleven seeded rows are removed one by one.
        Assert.Equal(11, CountOccurrences(text, "migrationBuilder.DeleteData("));
        for (var id = 2; id <= 12; id++)
        {
            Assert.Contains($"keyValue: {id}", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheRollbackRemovesTheJoinRowsThatReferenceTheRemovedCatalogueRows()
    {
        var text = MigrationText;

        // PatientMedicalConditions carries no foreign key to MedicalConditionTypes, so the
        // join rows are deleted before the catalogue rows they point at.
        Assert.Contains("DELETE FROM PatientMedicalConditions", text, StringComparison.Ordinal);
        Assert.Contains("WHERE MedicalConditionTypeId IN (2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMigrationDropsNoExistingColumn()
    {
        var text = MigrationText;

        Assert.DoesNotContain("DropTable(", text, StringComparison.Ordinal);
        Assert.DoesNotContain("RenameColumn(", text, StringComparison.Ordinal);
        Assert.DoesNotContain("AlterColumn(", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSnapshotCarriesTheNewModel()
    {
        var snapshot = SnapshotText;

        Assert.Contains("b.Property<string>(\"CardNumber\")", snapshot, StringComparison.Ordinal);
        Assert.Contains("b.HasIndex(\"CardNumber\");", snapshot, StringComparison.Ordinal);
        Assert.Contains("b.Property<bool>(\"RecentUltrasoundImaging\")", snapshot, StringComparison.Ordinal);
        Assert.Contains("b.Property<bool>(\"PrintResultPasswordEnabled\")", snapshot, StringComparison.Ordinal);
        Assert.Contains("b.Property<string>(\"PrintResultPasswordHash\")", snapshot, StringComparison.Ordinal);
        Assert.Contains("PrintResultPasswordEnabled = false,", snapshot, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSnapshotCarriesTheTwelveCatalogueRows()
    {
        var snapshot = SnapshotText;

        Assert.Contains("Name = \"حمل\"", snapshot, StringComparison.Ordinal);
        Assert.Contains("Name = \"هل يؤخذ علاج للسكر\"", snapshot, StringComparison.Ordinal);
        Assert.Contains("Name = \"مريض بالذئبة الحمراء\"", snapshot, StringComparison.Ordinal);

        // Twelve rows: ids 1 … 12.
        for (var id = 1; id <= 12; id++)
        {
            Assert.Contains($"Id = {id},", snapshot, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheEarlierMigrationsAreStillPresent()
    {
        var existing = new[]
        {
            "_BaselineDataModel.cs",
            "_RenamePkColumns.cs",
            "_AddTestCodeAndLifecycleColumns.cs",
            "_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs",
            "_AddPatientTestReferenceRangeSnapshots.cs",
            "_AddAnalyteProfileDomain.cs",
            "_AddPregnancyMedicalConditionTypeSeed.cs",
            "_AddInvoiceIssues.cs",
            "_FixCultureSensitivityCategoryOffByOne.cs",
            "_AddExternalEntityEmail.cs",
            "_AddBranchNumber.cs",
            "_AddCombinedReportPrintOptions.cs",
            "_AddCultureMicroscopyAndZone.cs",
            "_AddAntibioticMasterFields.cs",
            "_AddAntibioticCommercialName.cs",
            "_AddReportHeaderFooterImagesAndNormalRangeMarker.cs",
            "_AddReceiptFontAndLogoSettings.cs",
            "_AddReportItemTemplates.cs",
            "_AddReportItemLayout.cs",
            "_AddAbnormalResultColor.cs",
            "_AddProfileResultStyling.cs"
        };

        var files = Directory.EnumerateFiles(MigrationsDir, "*.cs")
            .Where(f => !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .ToList();

        foreach (var suffix in existing)
        {
            Assert.Contains(files, f => f!.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static int CountOccurrences(string text, string token)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
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

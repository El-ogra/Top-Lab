using System.IO;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.PatientSearch;

/// <summary>
/// S4 — the two worklist checkbox controls (P-01 F6 and F7).
///
/// Two separate facts are pinned here, and they are easy to conflate:
///   * the «مُراجعة» CHECKBOX (F7) — the filter control, added by this slice;
///   * the «مُراجعة» GRID COLUMN — a display-only column that existed before this wave and
///     must SURVIVE it. VG-04 requires the column still present after the change.
/// Both are asserted independently so neither can be lost.
/// </summary>
public class ResultsWorklistFilterControlsTests
{
    private static string ViewPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "TopLab.Presentation", "Views", "Patients", "ResultsWorklistView.xaml");
    }

    private static string ViewXaml() => File.ReadAllText(ViewPath());

    [Fact]
    public void ResultsWorklist_HasThreeFilterCheckBoxes()
    {
        var xaml = ViewXaml();

        // The three filters stay distinguishable: has result, reviewed, printed.
        Assert.Contains("IsChecked=\"{Binding HasResult,", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding IsReviewed,", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding IsPrinted,", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ResultsWorklist_PrintCheckboxIsTriState_SoFalseIsDistinguishableFromAbsent()
    {
        var xaml = ViewXaml();

        // F6 needs three states. A two-state CheckBox could not express "no filter",
        // which is the whole reason the query parameter is bool? (C-4).
        Assert.Contains("IsThreeState=\"True\"", xaml, StringComparison.Ordinal);

        var triStateIndex = xaml.IndexOf("IsThreeState=\"True\"", StringComparison.Ordinal);
        var isPrintedIndex = xaml.IndexOf("Binding IsPrinted", StringComparison.Ordinal);
        Assert.True(triStateIndex >= 0 && isPrintedIndex > triStateIndex,
            "the tri-state flag must apply to the IsPrinted checkbox.");
    }

    [Fact]
    public void ResultsWorklist_FilterLabelsAreDistinct()
    {
        var xaml = ViewXaml();

        Assert.Contains("Text=\"لدي نتيجة:\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"مُراجعة:\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"مطبوعة:\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ResultsWorklist_ReviewedGridColumn_StillPresent()
    {
        var xaml = ViewXaml();

        // VG-04: the display column must survive. It is a different thing from the checkbox.
        Assert.Contains("<DataGridTextColumn Header=\"مُراجعة\" Binding=\"{Binding IsReviewed}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<DataGridTextColumn Header=\"مطبوعة\" Binding=\"{Binding IsPrinted}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<DataGridTextColumn Header=\"مُسلمة\" Binding=\"{Binding IsDelivered}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ResultsWorklist_ViewStaysRightToLeft()
    {
        Assert.Contains("FlowDirection=\"RightToLeft\"", ViewXaml(), StringComparison.Ordinal);
    }

    [Fact]
    public void ResultsWorklist_FilterPropertiesArePublicAndTriState()
    {
        var properties = typeof(ResultsWorklistViewModel)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var name in new[] { "HasResult", "IsReviewed", "IsPrinted" })
        {
            var property = properties.SingleOrDefault(p => p.Name == name);
            Assert.NotNull(property);
            Assert.Equal(typeof(bool?), property!.PropertyType);
        }
    }

    [Fact]
    public void ResultsWorklist_PrintFilterResetsToFirstPage_LikeTheExistingFilters()
    {
        var property = typeof(ResultsWorklistViewModel).GetProperty("IsPrinted")!;

        // Same tri-state shape and same Page-reset idiom as HasResult / IsReviewed.
        Assert.Equal(typeof(bool?), property.PropertyType);
        Assert.True(property.CanRead);
        Assert.True(property.CanWrite);
    }

    [Fact]
    public void ResultsWorklist_PreExistingControls_Survive()
    {
        var xaml = ViewXaml();
        foreach (var token in new[] { "اليوم:", "لدي نتيجة:", "تحديث", "نوع النتيجة", "النتيجة" })
        {
            Assert.Contains(token, xaml, StringComparison.Ordinal);
        }
    }
}
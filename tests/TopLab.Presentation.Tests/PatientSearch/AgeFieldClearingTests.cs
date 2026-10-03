using System.IO;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.PatientSearch;

/// <summary>
/// P-02 S2 — D-2 (tri-state checkboxes) and D-4 (clearable age field).
///
/// D-4's real cause is a WPF conversion failure that cannot be exercised from a plain
/// xUnit test: a <c>TextBox</c> bound to <c>int?</c> cannot convert "" and the source keeps
/// its old value. The fix is that the property is now <c>string</c>-backed and parsing maps
/// empty/unparseable text to null, so these tests assert the parsing contract and the
/// resulting query shape directly.
/// </summary>
public class AgeFieldClearingTests
{
    // =====================================================================
    // D-4 — parsing: empty and unparseable text must mean "no bound"
    // =====================================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseAgeBound_EmptyMeansNoBound(string? text)
    {
        Assert.Null(PatientSearchViewModel.ParseAgeBound(text));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12abc")]
    [InlineData("-")]
    [InlineData("!@#")]
    public void ParseAgeBound_UnparseableMeansNoBound_AndDoesNotThrow(string text)
    {
        // A stray character widens the result set rather than breaking the screen.
        Assert.Null(PatientSearchViewModel.ParseAgeBound(text));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("7", 7)]
    [InlineData("42", 42)]
    [InlineData("  8  ", 8)]
    public void ParseAgeBound_ParsesValidNumbers(string text, int expected)
    {
        Assert.Equal(expected, PatientSearchViewModel.ParseAgeBound(text));
    }

    [Fact]
    public void AgeFromAndAgeTo_AreStringBacked()
    {
        // The type change IS the fix: an int? property cannot receive "" from a TextBox.
        var ageFrom = typeof(PatientSearchViewModel).GetProperty("AgeFrom")!;
        var ageTo = typeof(PatientSearchViewModel).GetProperty("AgeTo")!;

        Assert.Equal(typeof(string), ageFrom.PropertyType);
        Assert.Equal(typeof(string), ageTo.PropertyType);
    }

    [Fact]
    public void AgeFields_CanBeCleared()
    {
        // Clearing must produce an empty string / null, both of which parse to no bound.
        var vm = new PatientSearchViewModel(
            new FakeSender(), new ResultErrorPresenter(), new FakeNavigationService());

        vm.AgeFrom = "12";
        Assert.Equal("12", vm.AgeFrom);

        vm.AgeFrom = string.Empty;
        Assert.Equal(string.Empty, vm.AgeFrom);
        Assert.Null(PatientSearchViewModel.ParseAgeBound(vm.AgeFrom));

        vm.AgeFrom = null;
        Assert.Null(vm.AgeFrom);
        Assert.Null(PatientSearchViewModel.ParseAgeBound(vm.AgeFrom));
    }

    // =====================================================================
    // D-2 — all three worklist checkboxes are tri-state
    // =====================================================================

    private static string ResultsWorklistXaml()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(
            dir!.FullName, "src", "TopLab.Presentation", "Views", "Patients", "ResultsWorklistView.xaml"));
    }

    [Fact]
    public void ResultsWorklist_AllThreeCheckBoxesCarryIsThreeState()
    {
        var xaml = ResultsWorklistXaml();

        foreach (var bound in new[] { "HasResult", "IsReviewed", "IsPrinted" })
        {
            var bindingIndex = xaml.IndexOf($"Binding {bound}", StringComparison.Ordinal);
            Assert.True(bindingIndex >= 0, $"{bound} must be bound in the view");

            // The IsThreeState flag must precede the binding on the same element, so it
            // applies to THIS checkbox and not a different one.
            var flagIndex = xaml.LastIndexOf("IsThreeState=\"True\"", bindingIndex, StringComparison.Ordinal);
            Assert.True(flagIndex >= 0, $"{bound} must have IsThreeState=\"True\"");

            // Nothing but whitespace/margin may sit between the flag and its binding.
            var between = xaml[(flagIndex + "IsThreeState=\"True\"".Length)..bindingIndex];
            Assert.DoesNotContain("<CheckBox", between, StringComparison.Ordinal);
            Assert.DoesNotContain("<TextBlock", between, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ResultsWorklist_HasExactlyThreeTriStateCheckBoxes()
    {
        var xaml = ResultsWorklistXaml();
        var count = xaml.Split("IsThreeState=\"True\"").Length - 1;

        Assert.Equal(3, count);
    }

    [Fact]
    public void ResultsWorklist_ReviewedGridColumn_StillPresent()
    {
        // D-2 touches only the filter row; the display column must survive untouched.
        var xaml = ResultsWorklistXaml();

        Assert.Contains(
            "<DataGridTextColumn Header=\"مُراجعة\" Binding=\"{Binding IsReviewed}\"",
            xaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "<DataGridTextColumn Header=\"مطبوعة\" Binding=\"{Binding IsPrinted}\"",
            xaml,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ResultsWorklist_ViewStaysRightToLeft()
    {
        Assert.Contains("FlowDirection=\"RightToLeft\"", ResultsWorklistXaml(), StringComparison.Ordinal);
    }
}
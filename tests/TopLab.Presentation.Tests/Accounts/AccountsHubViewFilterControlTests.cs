using System.IO;
using System.Linq;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using Xunit;

namespace TopLab.Presentation.Tests.Accounts;

/// <summary>
/// Phase 5 — structural test: the filter ComboBox exists in the Patient Samples tab
/// of the Accounts hub XAML.
/// </summary>
public class AccountsHubViewFilterControlTests
{
    private static string ViewPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "TopLab.Presentation", "Views", "Accounts", "AccountsHubView.xaml");
    }

    private static string ViewXaml() => File.ReadAllText(ViewPath());

    [Fact]
    public void AccountsHubView_PatientSamplesTab_HasReceivableFilterComboBox()
    {
        var xaml = ViewXaml();

        // The filter label exists
        Assert.Contains("تصفية الرصيد:", xaml, StringComparison.Ordinal);

        // The ComboBox binds to the correct properties
        Assert.Contains("ReceivableFilterOptions", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedValue=\"{Binding ReceivableFilter", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedValuePath=\"Value\"", xaml, StringComparison.Ordinal);
        Assert.Contains("DisplayMemberPath=\"Label\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountsHubView_FilterOptions_HaveFourChoices()
    {
        var values = Enum.GetValues(typeof(ReceivableFilter)).Cast<ReceivableFilter>().ToList();
        Assert.Equal(4, values.Count);
        Assert.Contains(ReceivableFilter.All, values);
        Assert.Contains(ReceivableFilter.UnpaidOnly, values);
        Assert.Contains(ReceivableFilter.DiscountedOnly, values);
        Assert.Contains(ReceivableFilter.PaidOnly, values);
    }
}

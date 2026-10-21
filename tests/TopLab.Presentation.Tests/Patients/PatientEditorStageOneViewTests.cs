using System.IO;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// STAGE-01: the registration screen carries the new controls — the visit
/// dates, the card number, the read-only sample-type summary, the fifteen
/// medical-history questions, the two additional-information actions and the
/// print-result password.
/// </summary>
public class PatientEditorStageOneViewTests
{
    private static string ViewText => File.ReadAllText(Path.Combine(
        FindSolutionRoot(), "src", "TopLab.Presentation", "Views", "Patients", "PatientEditorView.xaml"));

    [Fact]
    public void TheHeader_CarriesTheEntryDateAndTheReceiptDate()
    {
        var xaml = ViewText;

        Assert.Contains("Text=\"{Binding RegistrationDateText", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding PickupDateText", xaml, StringComparison.Ordinal);
        Assert.Contains("تاريخ الدخول:", xaml, StringComparison.Ordinal);
        Assert.Contains("تاريخ الاستلام:", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TheIdentityBlock_CarriesTheCardNumberAndTheSampleTypeSummary()
    {
        var xaml = ViewText;

        Assert.Contains("Text=\"{Binding CardNumber", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding SampleTypeSummary}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("رقم بطاقة / الملف:", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TheIdentityBlock_CarriesTheTwoAdditionalInformationActions()
    {
        var xaml = ViewText;

        Assert.Contains("Command=\"{Binding ShowTreatingDoctorDetailsCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ShowReferralEntityDetailsCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("معلومات اضافية", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TheMedicalHistoryBlock_CarriesTheFifteenQuestions()
    {
        var xaml = ViewText;

        // The fasting pair, the catalogue toggles and the single imaging question.
        Assert.Contains("IsChecked=\"{Binding IsFastingIndicated}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding FastingHoursText", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding MedicationConditions}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ItemsSource=\"{Binding ConditionToggles}\"", xaml, StringComparison.Ordinal);

        // The imaging question is ONE question with two answers.
        Assert.Contains("IsChecked=\"{Binding RecentContrastImaging}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding RecentUltrasoundImaging}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("هل تم عمل اشاعة بالصبغة او الموجات الصوتية خلال يومين", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommunicationBlock_CarriesThePrintResultPassword()
    {
        var xaml = ViewText;

        Assert.Contains("IsChecked=\"{Binding PrintResultPasswordEnabled}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PrintResultPasswordBox", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding SavePrintResultPasswordCommand}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLabHub_CarriesTheMedicalConditionCatalogueTab()
    {
        var xaml = File.ReadAllText(Path.Combine(
            FindSolutionRoot(), "src", "TopLab.Presentation", "Views", "Lab", "LabHubView.xaml"));

        Assert.Contains("Content=\"{Binding MedicalConditionTypes}\"", xaml, StringComparison.Ordinal);
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }
}

using System.IO;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Navigation;

/// <summary>WP-02: every registered entry screen must have a navigation caller.</summary>
public class OrphanedViewModelTests
{
    private static readonly string[] RootViewModels =
    {
        "ShellViewModel",
        "LoginViewModel",
        "MainWindow"
    };

    private static IEnumerable<string> SourceFiles()
    {
        var root = FindRepoRoot();
        return Directory.EnumerateFiles(
            Path.Combine(root, "src", "TopLab.Presentation"),
            "*.cs",
            SearchOption.AllDirectories);
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

    private static bool HasNavigationCaller(string vmName)
    {
        var needle = $"NavigateTo<{vmName}>";
        return SourceFiles().Any(f => File.ReadAllText(f).Contains(needle, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(nameof(BlankReportViewModel))]
    [InlineData(nameof(HistoryReportsViewModel))]
    [InlineData(nameof(SimpleResultEntryViewModel))]
    [InlineData(nameof(ProfileEntryViewModel))]
    [InlineData(nameof(CultureEntryViewModel))]
    [InlineData(nameof(ResultsWorklistViewModel))]
    [InlineData(nameof(CombinedReportViewModel))]
    public void OrphanedViewModel_EachRegisteredEntryScreen_HasNavigationCaller(string vmName)
    {
        Assert.True(HasNavigationCaller(vmName), $"No NavigateTo<{vmName}> found in Presentation sources.");
    }

    [Fact]
    public void OrphanedViewModel_BlankReport_HasNavigationCaller()
    {
        Assert.True(HasNavigationCaller(nameof(BlankReportViewModel)));
    }

    [Fact]
    public void OrphanedViewModel_HistoryReports_HasNavigationCaller()
    {
        Assert.True(HasNavigationCaller(nameof(HistoryReportsViewModel)));
    }

    [Fact]
    public void OrphanedViewModel_ResultEntryScreens_ReachableFromWorklist()
    {
        // OpenDetailAsync routes Simple/Profile/Culture entry screens.
        var vmSource = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "src", "TopLab.Presentation", "ViewModels", "Patients", "ResultsWorklistViewModel.cs"));

        Assert.Contains("NavigateTo<SimpleResultEntryViewModel>", vmSource, StringComparison.Ordinal);
        Assert.Contains("NavigateTo<ProfileEntryViewModel>", vmSource, StringComparison.Ordinal);
        Assert.Contains("NavigateTo<CultureEntryViewModel>", vmSource, StringComparison.Ordinal);
        Assert.Contains("OpenDetailCommand", vmSource, StringComparison.Ordinal);
    }

    private sealed class RecordingNavigation : TopLab.Presentation.Common.Navigation.INavigationService
    {
        public TopLab.Presentation.Common.ViewModelBase? CurrentViewModel { get; private set; }
        public event Action<TopLab.Presentation.Common.ViewModelBase?>? Navigated;
        public List<string> Visited { get; } = new();

        public void NavigateTo<TViewModel>() where TViewModel : TopLab.Presentation.Common.ViewModelBase
        {
            Visited.Add(typeof(TViewModel).Name);
        }

        public void NavigateTo(TopLab.Presentation.Common.ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
            Navigated?.Invoke(viewModel);
        }
    }

    [Fact]
    public void ResultsWorklist_OpenDetail_RoutesByResultKind()
    {
        var nav = new RecordingNavigation();
        var vm = new ResultsWorklistViewModel(
            new FakeSender(),
            new TopLab.Presentation.Common.ErrorPresentation.ResultErrorPresenter(),
            nav);

        var simple = new TopLab.Application.Features.ResultsEntry.Common.ResultWorklistItemDto(
            10, 1, "Ali", "L1", "T", "T1", 0, false, false, false, null, null, true, false, false, 1, DateTime.UtcNow);
        var profile = simple with { PatientTestId = 11, ResultKind = 1 };
        var culture = simple with { PatientTestId = 12, ResultKind = 2, IsCultureType = true };

        vm.OpenDetailCommand.Execute(simple);
        vm.OpenDetailCommand.Execute(profile);
        vm.OpenDetailCommand.Execute(culture);

        Assert.Contains(nameof(SimpleResultEntryViewModel), nav.Visited);
        Assert.Contains(nameof(ProfileEntryViewModel), nav.Visited);
        Assert.Contains(nameof(CultureEntryViewModel), nav.Visited);
    }

    [Fact]
    public void ResultsWorklist_OpenDetail_NullParameter_ShowsMessageNotCrash()
    {
        var vm = new ResultsWorklistViewModel(
            new FakeSender(),
            new TopLab.Presentation.Common.ErrorPresentation.ResultErrorPresenter(),
            new FakeNavigationService());

        vm.OpenDetailCommand.Execute(null);

        Assert.Equal("اختر مريضاً من القائمة أولاً.", vm.StatusMessage);
    }

    [Fact]
    public void PatientsHub_ShowsFourPlusTwoEntryButtons()
    {
        var hub = new PatientsHubViewModel(new FakeNavigationService());

        Assert.True(hub.AddEditPatientEnabled);
        Assert.True(hub.EnterResultsEnabled);
        Assert.True(hub.SearchPatientEnabled);
        Assert.True(hub.DeliverResultsEnabled);
        Assert.True(hub.BlankReportEnabled);
        Assert.True(hub.HistoryReportsEnabled);
        Assert.NotNull(hub.OpenBlankReportCommand);
        Assert.NotNull(hub.OpenHistoryReportsCommand);
    }
}

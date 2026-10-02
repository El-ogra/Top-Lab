using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests;

/// <summary>W-02 S12 (WP-10): multi-patient entry, null-safety, same-test insertion filter.</summary>
public class HistoryReportsViewModelTests
{
    private static HistoryReportsViewModel Screen(FakeSender sender) =>
        new(sender, new ResultErrorPresenter(), new FakeDialogService(), new FakeNavigationService());

    private static PatientHistoryDto History(params HistoryEntryDto[] entries) =>
        new(1, "P", null, "ByLabCode", true, entries);

    private static HistoryEntryDto Entry(int ptId, int testId, string testName) =>
        new(ptId, 1, testId, testName, "T", 0, "5", null, true, null, null);

    [Fact]
    public async Task HistoryReports_MultiPatientIds_IsPublicObservable()
    {
        var sender = new FakeSender();
        var vm = Screen(sender);

        vm.MultiPatientIds.Add(1);
        vm.MultiPatientIds.Add(2);

        Assert.Equal(new[] { 1, 2 }, vm.MultiPatientIds);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task HistoryReports_SeparateLoadWithNullEntries_ClearsAndShowsError()
    {
        var sender = new FakeSender()
            .WithResponse(
                new GetPatientTestHistoryQuery(1),
                Result<PatientHistoryDto>.Success(History()))
            .WithResponse(
                new GetSeparateHistoryReportQuery(1),
                Result<PatientHistoryDto>.Success(null!));
        var vm = Screen(sender);
        await vm.LoadAsync(1);

        vm.LoadSeparateCommand.Execute(null);
        await WaitForIdleAsync(vm);

        Assert.Empty(vm.Entries);
        Assert.Equal("لا توجد بيانات تاريخية لهذا المريض.", vm.ErrorMessage);
    }

    private static async Task WaitForIdleAsync(HistoryReportsViewModel vm)
    {
        for (var i = 0; i < 200 && (vm.IsBusy || (vm.Entries.Count == 0 && string.IsNullOrEmpty(vm.ErrorMessage))); i++)
        {
            await Task.Delay(25);
        }
    }

    [Fact]
    public async Task GetSeparateHistoryReportQuery_StillResolves()
    {
        Assert.NotNull(typeof(GetSeparateHistoryReportQuery));
        Assert.NotNull(typeof(GetPatientTestHistoryQuery));
        Assert.NotNull(typeof(GetMultiPatientHistoryQuery));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task InsertHistoryDialog_ExcludesOtherTests()
    {
        var sender = new FakeSender()
            .WithResponse(
                new GetPatientTestHistoryQuery(1),
                Result<PatientHistoryDto>.Success(History(
                    Entry(11, 1, "Glucose"),
                    Entry(12, 2, "CBC"))));
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), new FakeDialogService());

        await vm.LoadAsync(1, 10, "Glucose");

        Assert.Equal(new[] { 11 }, vm.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public async Task InsertHistoryDialog_IncludesSameTestAcrossVisits()
    {
        var sender = new FakeSender()
            .WithResponse(
                new GetPatientTestHistoryQuery(1),
                Result<PatientHistoryDto>.Success(History(
                    Entry(11, 1, "Glucose"),
                    Entry(12, 1, "Glucose"))));
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), new FakeDialogService());

        await vm.LoadAsync(1, 10, "Glucose");

        Assert.Equal(2, vm.Entries.Count);
    }
}

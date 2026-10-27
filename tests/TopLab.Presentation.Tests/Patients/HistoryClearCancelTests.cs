using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Tests.Common;
using TopLab.Presentation.ViewModels.Patients;
using Xunit;

namespace TopLab.Presentation.Tests.Patients;

/// <summary>
/// GAP-27 (refs 1.15.4.5 "حذف الكل" and 1.15.3.4 "الغاء الحالى"):
/// clearing the history-report selection keeps the entries and the filters, and
/// abandoning the insert-history operation never dispatches
/// <see cref="InsertHistoryResultCommand"/>.
/// </summary>
public class HistoryClearCancelTests
{
    private sealed class ConfirmingDialogService : IDialogService
    {
        public ConfirmingDialogService(bool confirmationResult = true)
        {
            ConfirmationResult = confirmationResult;
        }

        public bool ConfirmationResult { get; set; }
        public List<(string Title, string Message)> ConfirmationCalls { get; } = new();

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            ConfirmationCalls.Add((title, message));
            return Task.FromResult(ConfirmationResult);
        }

        public Task ShowErrorAsync(string message) => Task.CompletedTask;
        public Task<bool> ShowSecondaryPasswordDialogAsync() => Task.FromResult(false);
        public Task<string?> PickBackupFolderAsync(string initialDirectory) => Task.FromResult<string?>(null);
        public Task<string?> PickBackupFileAsync() => Task.FromResult<string?>(null);
        public Task<string?> PickPdfSavePathAsync(string? suggestedFileName = null) => Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Records every dispatched request and delegates canned responses to the
    /// shared <see cref="FakeSender"/>. <c>gateHistory</c> holds the history query
    /// until <see cref="HistoryGate"/> completes, so abandonment during a load can
    /// be tested deterministically.
    /// </summary>
    private sealed class RecordingSender : ISender
    {
        private readonly FakeSender _inner = new();
        private readonly bool _gateHistory;

        public RecordingSender(bool gateHistory = false)
        {
            _gateHistory = gateHistory;
        }

        public List<object> SentRequests { get; } = new();
        public TaskCompletionSource HistoryGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public RecordingSender WithResponse<TResponse>(IRequest<TResponse> request, TResponse response)
        {
            _inner.WithResponse(request, response);
            return this;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            SentRequests.Add(request);
            if (_gateHistory && request is GetPatientTestHistoryQuery)
            {
                await HistoryGate.Task;
            }

            return await _inner.Send(request, cancellationToken);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("RecordingSender does not support non-generic Send.");

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException("RecordingSender does not support void Send.");

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("RecordingSender does not support streams.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("RecordingSender does not support streams.");
    }

    private static HistoryEntryDto Entry(int patientTestId, int testId, string testName) =>
        new(patientTestId, 1, testId, testName, "T", 0, "5", null, true, null, null);

    private static PatientHistoryDto History(params HistoryEntryDto[] entries) =>
        new(1, "P", null, "ByLabCode", true, entries);

    private static (HistoryReportsViewModel Vm, RecordingSender Sender) HistoryScreen(params HistoryEntryDto[] entries)
    {
        var sender = new RecordingSender();
        sender.WithResponse(
            new GetPatientTestHistoryQuery(1),
            Result<PatientHistoryDto>.Success(History(entries)));
        var vm = new HistoryReportsViewModel(
            sender,
            new ResultErrorPresenter(),
            new ConfirmingDialogService(),
            new FakeNavigationService());
        return (vm, sender);
    }

    private static RecordingSender InsertSender(params HistoryEntryDto[] entries)
    {
        var sender = new RecordingSender();
        sender.WithResponse(
            new GetPatientTestHistoryQuery(1),
            Result<PatientHistoryDto>.Success(History(entries)));
        sender.WithResponse(
            new InsertHistoryResultCommand(10, 11),
            Result<CombinedReportDto>.Success(
                new CombinedReportDto(1, "P", null, Array.Empty<CombinedReportLineDto>())));
        return sender;
    }

    // ------------------------------------------------------------------
    // Clear selection (1.15.4.5)
    // ------------------------------------------------------------------

    [Fact]
    public async Task History_ClearSelection_ClearsSelectionAndKeepsEntries()
    {
        var (vm, _) = HistoryScreen(Entry(11, 1, "Glucose"), Entry(12, 2, "CBC"));
        await vm.LoadAsync(1);
        vm.SelectedEntry = vm.Entries[0];

        vm.ClearSelectionCommand.Execute(null);

        Assert.Null(vm.SelectedEntry);
        Assert.Equal(new[] { 11, 12 }, vm.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public async Task History_ClearSelection_PreservesFiltersAndMode()
    {
        var (vm, _) = HistoryScreen(Entry(11, 1, "Glucose"));
        await vm.LoadAsync(1);
        vm.FromDate = new DateTime(2026, 1, 1);
        vm.ToDate = new DateTime(2026, 3, 1);
        vm.TestIdText = "5";
        vm.PrintSeparately = true;
        vm.SelectedMode = 2;
        vm.SelectedEntry = vm.Entries[0];

        vm.ClearSelectionCommand.Execute(null);

        Assert.Null(vm.SelectedEntry);
        Assert.Equal(new DateTime(2026, 1, 1), vm.FromDate);
        Assert.Equal(new DateTime(2026, 3, 1), vm.ToDate);
        Assert.Equal("5", vm.TestIdText);
        Assert.True(vm.PrintSeparately);
        Assert.Equal(2, vm.SelectedMode);
        Assert.Single(vm.Entries);
    }

    [Fact]
    public async Task History_ClearSelection_WhenNothingSelected_IsSafeNoOp()
    {
        var (vm, _) = HistoryScreen(Entry(11, 1, "Glucose"));
        await vm.LoadAsync(1);
        vm.SelectedEntry = null;

        var ex = Record.Exception(() => vm.ClearSelectionCommand.Execute(null));

        Assert.Null(ex);
        Assert.Null(vm.SelectedEntry);
        Assert.Single(vm.Entries);
    }

    [Fact]
    public async Task History_ClearSelection_DoesNotDispatchAnyMutatingRequest()
    {
        var (vm, sender) = HistoryScreen(Entry(11, 1, "Glucose"));
        await vm.LoadAsync(1);
        vm.SelectedEntry = vm.Entries[0];

        vm.ClearSelectionCommand.Execute(null);

        // Only the history load was ever sent; clearing dispatched nothing.
        var request = Assert.Single(sender.SentRequests);
        Assert.IsType<GetPatientTestHistoryQuery>(request);
        Assert.Empty(sender.SentRequests.OfType<InsertHistoryResultCommand>());
    }

    [Fact]
    public async Task History_ClearSelectionCommand_CanExecuteTracksSelection()
    {
        var (vm, _) = HistoryScreen(Entry(11, 1, "Glucose"));
        await vm.LoadAsync(1);
        Assert.False(vm.ClearSelectionCommand.CanExecute(null));

        vm.SelectedEntry = vm.Entries[0];
        Assert.True(vm.ClearSelectionCommand.CanExecute(null));

        vm.ClearSelectionCommand.Execute(null);
        Assert.False(vm.ClearSelectionCommand.CanExecute(null));
    }

    /// <summary>
    /// GAP-27-AUDIT-01 / -03: the ViewModel test above proves only that
    /// <see cref="HistoryReportsViewModel.SelectedEntry"/> becomes null. The WPF
    /// grid clears every selected row only because that property is bound
    /// two-way to <c>DataGrid.SelectedItem</c> while the grid is in
    /// <c>Extended</c> selection mode. This test locks that wiring in place: a
    /// OneWay binding or a single-selection grid would silently reintroduce the
    /// reported multi-selection gap.
    /// </summary>
    [Fact]
    public void History_ClearSelection_IsWiredThroughTheExtendedGridTwoWaySelectionBinding()
    {
        var xamlPath = FindRepositoryFile(
            "src", "TopLab.Presentation", "Views", "Patients", "HistoryReportsView.xaml");
        var document = XDocument.Load(xamlPath);

        var grid = Assert.Single(document.Descendants(), e => e.Name.LocalName == "DataGrid");

        var itemsSource = (string?)grid.Attribute("ItemsSource");
        Assert.NotNull(itemsSource);
        Assert.Contains("Entries", itemsSource!, StringComparison.Ordinal);

        var selectedItemBinding = (string?)grid.Attribute("SelectedItem");
        Assert.NotNull(selectedItemBinding);
        Assert.Contains("SelectedEntry", selectedItemBinding!, StringComparison.Ordinal);
        Assert.Contains("TwoWay", selectedItemBinding!, StringComparison.Ordinal);
        Assert.Equal("Extended", (string?)grid.Attribute("SelectionMode"));

        var clearButton = Assert.Single(document.Descendants(), e =>
            e.Name.LocalName == "Button" && (string?)e.Attribute("Content") == "حذف الكل");
        Assert.Equal("{Binding ClearSelectionCommand}", (string?)clearButton.Attribute("Command"));
    }

    private static string FindRepositoryFile(params string[] relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "TopLab.Presentation")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(new[] { dir!.FullName }.Concat(relativePath).ToArray());
    }

    // ------------------------------------------------------------------
    // Cancel / abandon the history operation (1.15.3.4)
    // ------------------------------------------------------------------

    [Fact]
    public async Task InsertDialog_Cancel_AbandonsWithoutDispatchingInsert()
    {
        var sender = InsertSender(Entry(11, 1, "Glucose"));
        var dialogs = new ConfirmingDialogService();
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), dialogs);
        await vm.LoadAsync(1, 10, "Glucose");
        vm.SelectedEntry = vm.Entries[0];

        vm.Cancel();

        Assert.Null(vm.SelectedEntry);
        Assert.Null(vm.InsertedReport);
        Assert.Empty(dialogs.ConfirmationCalls);
        Assert.DoesNotContain(sender.SentRequests, r => r is InsertHistoryResultCommand);
    }

    [Fact]
    public async Task InsertDialog_AfterCancel_InsertReturnsFalseWithoutConfirmOrDispatch()
    {
        var sender = InsertSender(Entry(11, 1, "Glucose"));
        var dialogs = new ConfirmingDialogService();
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), dialogs);
        await vm.LoadAsync(1, 10, "Glucose");
        vm.SelectedEntry = vm.Entries[0];
        vm.Cancel();

        var inserted = await vm.InsertAsync();

        Assert.False(inserted);
        Assert.Empty(dialogs.ConfirmationCalls);
        Assert.DoesNotContain(sender.SentRequests, r => r is InsertHistoryResultCommand);
        Assert.Null(vm.InsertedReport);
    }

    [Fact]
    public void InsertDialog_Cancel_WhenNoOperationActive_IsSafe()
    {
        var sender = InsertSender();
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), new ConfirmingDialogService());

        var ex = Record.Exception(() =>
        {
            vm.Cancel();
            vm.Cancel();
        });

        Assert.Null(ex);
        Assert.Null(vm.SelectedEntry);
        Assert.Empty(sender.SentRequests);
    }

    [Fact]
    public async Task InsertDialog_Cancel_DuringLoad_DoesNotRepopulateOrInsert()
    {
        var sender = new RecordingSender(gateHistory: true);
        sender.WithResponse(
            new GetPatientTestHistoryQuery(1),
            Result<PatientHistoryDto>.Success(History(Entry(11, 1, "Glucose"))));
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), new ConfirmingDialogService());

        var load = vm.LoadAsync(1, 10, "Glucose");
        vm.Cancel();
        sender.HistoryGate.SetResult();
        await load;

        Assert.Empty(vm.Entries);
        Assert.Null(vm.SelectedEntry);
        Assert.DoesNotContain(sender.SentRequests, r => r is InsertHistoryResultCommand);
    }

    [Fact]
    public async Task InsertDialog_ConfirmationDeclined_DoesNotInsert()
    {
        var sender = InsertSender(Entry(11, 1, "Glucose"));
        var dialogs = new ConfirmingDialogService(confirmationResult: false);
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), dialogs);
        await vm.LoadAsync(1, 10, "Glucose");
        vm.SelectedEntry = vm.Entries[0];

        var inserted = await vm.InsertAsync();

        Assert.False(inserted);
        Assert.Single(dialogs.ConfirmationCalls);
        Assert.DoesNotContain(sender.SentRequests, r => r is InsertHistoryResultCommand);
        Assert.Null(vm.InsertedReport);
        // The confirmation cancel is NOT the operation cancel: the selection stays
        // so the user can adjust it and try again.
        Assert.NotNull(vm.SelectedEntry);
    }

    [Fact]
    public async Task InsertDialog_ConfirmedInsert_StillDispatchesInsertHistoryResultCommand()
    {
        var sender = InsertSender(Entry(11, 1, "Glucose"));
        var dialogs = new ConfirmingDialogService(confirmationResult: true);
        var vm = new InsertHistoryDialogViewModel(sender, new ResultErrorPresenter(), dialogs);
        await vm.LoadAsync(1, 10, "Glucose");
        vm.SelectedEntry = vm.Entries[0];

        var inserted = await vm.InsertAsync();

        Assert.True(inserted);
        Assert.Single(dialogs.ConfirmationCalls);
        var command = Assert.Single(sender.SentRequests.OfType<InsertHistoryResultCommand>());
        Assert.Equal(10, command.PatientTestId);
        Assert.Equal(11, command.SourcePatientTestId);
        Assert.NotNull(vm.InsertedReport);
    }
}

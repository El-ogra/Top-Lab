using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.RenameWorkGroupLog;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetWorkGroupLogs;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class WorkGroupLogItemRow
{
    public WorkGroupLogItemRow(int testId, string testName)
    {
        TestId = testId;
        TestName = testName;
    }

    public int TestId { get; }
    public string TestName { get; }
}

/// <summary>
/// M12 work-group-logs tab (S-02 Slice 3): logs list with an items-grid
/// editor. No delete affordance exists (no delete command in code).
/// </summary>
public sealed class WorkGroupLogsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<WorkGroupLogDto> _logs = new();
    private WorkGroupLogDto? _selectedLog;
    private string _logName = string.Empty;
    private ObservableCollection<WorkGroupLogItemRow> _items = new();
    private ObservableCollection<TestSummaryDto> _catalogTests = new();
    private TestSummaryDto? _pickedTest;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public WorkGroupLogsViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        LoadLogsCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => ClearEditor());
        SaveLogCommand = new AsyncRelayCommand(_ => SaveLogAsync());
        AddItemCommand = new AsyncRelayCommand(_ => AddItemAsync());
        RemoveItemCommand = new RelayCommand(p => RemoveItem(p as WorkGroupLogItemRow));
        SaveItemsCommand = new AsyncRelayCommand(_ => SaveItemsAsync());
    }

    public ObservableCollection<WorkGroupLogDto> Logs { get => _logs; private set => SetProperty(ref _logs, value); }

    public WorkGroupLogDto? SelectedLog
    {
        get => _selectedLog;
        set
        {
            if (SetProperty(ref _selectedLog, value) && value is not null)
            {
                LogName = value.Name;
                Items = new ObservableCollection<WorkGroupLogItemRow>(
                    value.Items.Select(i => new WorkGroupLogItemRow(i.TestId, i.TestName)));
            }
        }
    }

    public string LogName { get => _logName; set => SetProperty(ref _logName, value); }
    public ObservableCollection<WorkGroupLogItemRow> Items { get => _items; private set => SetProperty(ref _items, value); }
    public ObservableCollection<TestSummaryDto> CatalogTests { get => _catalogTests; private set => SetProperty(ref _catalogTests, value); }
    public TestSummaryDto? PickedTest { get => _pickedTest; set => SetProperty(ref _pickedTest, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadLogsCommand { get; }
    public RelayCommand NewCommand { get; }
    public AsyncRelayCommand SaveLogCommand { get; }
    public AsyncRelayCommand AddItemCommand { get; }
    public RelayCommand RemoveItemCommand { get; }
    public AsyncRelayCommand SaveItemsCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var logsResult = await _mediator.Send(new GetWorkGroupLogsQuery());
            if (logsResult.IsSuccess && logsResult.Value is not null)
            {
                Logs = new ObservableCollection<WorkGroupLogDto>(logsResult.Value);
            }
            else if (logsResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(logsResult.Error);
            }

            var catalogResult = await _mediator.Send(new SearchTestCatalogQuery(null, null, IncludeInactive: false));
            if (catalogResult.IsSuccess && catalogResult.Value is not null)
            {
                CatalogTests = new ObservableCollection<TestSummaryDto>(catalogResult.Value);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearEditor()
    {
        SelectedLog = null;
        LogName = string.Empty;
        Items = new ObservableCollection<WorkGroupLogItemRow>();
    }

    public async Task SaveLogAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (SelectedLog is null)
            {
                var result = await _mediator.Send(new CreateWorkGroupLogCommand(LogName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم إنشاء السجل.";
            }
            else
            {
                var result = await _mediator.Send(new RenameWorkGroupLogCommand(SelectedLog.Id, LogName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ السجل.";
            }

            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task AddItemAsync()
    {
        if (PickedTest is null)
        {
            ErrorMessage = "اختر تحليلاً لإضافته.";
            return Task.CompletedTask;
        }

        if (Items.Any(i => i.TestId == PickedTest.Id))
        {
            ErrorMessage = "هذا التحليل مضاف بالفعل";
            return Task.CompletedTask;
        }

        ErrorMessage = string.Empty;
        Items.Add(new WorkGroupLogItemRow(PickedTest.Id, PickedTest.Name));
        return Task.CompletedTask;
    }

    private void RemoveItem(WorkGroupLogItemRow? row)
    {
        if (row is not null)
        {
            Items.Remove(row);
        }
    }

    public async Task SaveItemsAsync()
    {
        if (SelectedLog is null)
        {
            ErrorMessage = "اختر سجلاً لحفظ بنوده.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new SaveWorkGroupLogItemsCommand(
                SelectedLog.Id, Items.Select(i => i.TestId).ToList()));
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                return;
            }

            StatusMessage = "تم حفظ البنود.";
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

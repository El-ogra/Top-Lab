using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTestGroup;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class TestGroupRow : ViewModelBase
{
    private int _testCount;

    public TestGroupRow(TestGroupDto group)
    {
        Group = group;
    }

    public TestGroupDto Group { get; }

    public int TestCount
    {
        get => _testCount;
        set => SetProperty(ref _testCount, value);
    }
}

/// <summary>
/// M12 test groups tab (S-02 Slice 3): table with derived test counts,
/// name editor, and deactivate/reactivate.
/// </summary>
public sealed class TestGroupsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<TestGroupRow> _rows = new();
    private TestGroupRow? _selectedRow;
    private string _groupName = string.Empty;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public TestGroupsViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        LoadGroupsCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        NewCommand = new RelayCommand(_ => ClearEditor());
        ToggleActiveCommand = new AsyncRelayCommand((p, _) => ToggleActiveAsync(p as TestGroupRow));
    }

    public ObservableCollection<TestGroupRow> Rows { get => _rows; private set => SetProperty(ref _rows, value); }

    public TestGroupRow? SelectedRow
    {
        get => _selectedRow;
        set
        {
            if (SetProperty(ref _selectedRow, value) && value is not null)
            {
                GroupName = value.Group.Name;
            }
        }
    }

    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadGroupsCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand NewCommand { get; }
    public AsyncRelayCommand ToggleActiveCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var groupsResult = await _mediator.Send(new GetTestGroupsQuery(IncludeInactive: true));
            if (!groupsResult.IsSuccess || groupsResult.Value is null)
            {
                if (groupsResult.Error is not null)
                {
                    ErrorMessage = _presenter.Present(groupsResult.Error);
                }

                return;
            }

            var catalogResult = await _mediator.Send(new SearchTestCatalogQuery(null, null, IncludeInactive: true));
            var counts = new Dictionary<int, int>();
            if (catalogResult.IsSuccess && catalogResult.Value is not null)
            {
                foreach (var t in catalogResult.Value)
                {
                    if (t.TestGroupId.HasValue)
                    {
                        counts[t.TestGroupId.Value] = counts.GetValueOrDefault(t.TestGroupId.Value) + 1;
                    }
                }
            }

            var rows = groupsResult.Value
                .Select(g => new TestGroupRow(g) { TestCount = counts.GetValueOrDefault(g.Id) })
                .ToList();
            Rows = new ObservableCollection<TestGroupRow>(rows);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearEditor()
    {
        SelectedRow = null;
        GroupName = string.Empty;
    }

    public async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (SelectedRow is null)
            {
                var result = await _mediator.Send(new CreateTestGroupCommand(GroupName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم إنشاء المجموعة.";
            }
            else
            {
                var result = await _mediator.Send(new UpdateTestGroupCommand(SelectedRow.Group.Id, GroupName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ المجموعة.";
            }

            ClearEditor();
            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ToggleActiveAsync(TestGroupRow? row)
    {
        if (row is null)
        {
            return;
        }

        if (row.Group.IsActive)
        {
            bool confirm = await _dialogs.ShowConfirmationAsync(
                "تعطيل المجموعة",
                $"سيتم تعطيل المجموعة \"{row.Group.Name}\" — متابعة؟");
            if (!confirm)
            {
                return;
            }

            var result = await _mediator.Send(new DeactivateTestGroupCommand(row.Group.Id));
            if (!result.IsSuccess && result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                return;
            }
        }
        else
        {
            bool confirm = await _dialogs.ShowConfirmationAsync(
                "إعادة تفعيل المجموعة",
                $"سيتم إعادة تفعيل المجموعة \"{row.Group.Name}\" — متابعة؟");
            if (!confirm)
            {
                return;
            }

            var result = await _mediator.Send(new ReactivateTestGroupCommand(row.Group.Id));
            if (!result.IsSuccess && result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                return;
            }
        }

        await LoadAsync();
    }
}

using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class CustomGroupItemRow : ViewModelBase
{
    private decimal _editPrice;
    private bool _isSaved;

    public CustomGroupItemRow(CustomGroupItemDto item)
    {
        Item = item;
        _editPrice = item.Price;
    }

    public CustomGroupItemDto Item { get; }

    public decimal EditPrice
    {
        get => _editPrice;
        set => SetProperty(ref _editPrice, value);
    }

    public bool IsSaved
    {
        get => _isSaved;
        set => SetProperty(ref _isSaved, value);
    }
}

/// <summary>
/// Custom-groups master-detail tab (S-02 Slice 7): mirrors the price-lists
/// pattern with row-by-row save and a «محفوظ» badge.
/// </summary>
public sealed class CustomGroupsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<CustomGroupSummaryDto> _groups = new();
    private CustomGroupSummaryDto? _selectedGroup;
    private ObservableCollection<CustomGroupItemRow> _items = new();
    private string _groupName = string.Empty;
    private ObservableCollection<TestSummaryDto> _catalogTests = new();
    private TestSummaryDto? _pickedTest;
    private decimal _newItemPrice;
    private int _lastSavedTestId;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public CustomGroupsViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        LoadGroupsCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => ClearEditor());
        SaveGroupCommand = new AsyncRelayCommand(_ => SaveGroupAsync());
        DeleteGroupCommand = new AsyncRelayCommand(_ => DeleteGroupAsync());
        AddItemCommand = new AsyncRelayCommand(_ => AddItemAsync());
        SaveItemPriceCommand = new AsyncRelayCommand((p, _) => SaveItemPriceAsync(p as CustomGroupItemRow));
        RemoveItemCommand = new AsyncRelayCommand((p, _) => RemoveItemAsync(p as CustomGroupItemRow));
    }

    public ObservableCollection<CustomGroupSummaryDto> Groups { get => _groups; private set => SetProperty(ref _groups, value); }

    public CustomGroupSummaryDto? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
            {
                GroupName = value?.Name ?? string.Empty;
                _ = ReloadItemsAsync();
            }
        }
    }

    public ObservableCollection<CustomGroupItemRow> Items
    {
        get => _items;
        private set
        {
            if (SetProperty(ref _items, value))
            {
                OnPropertyChanged(nameof(ShowItemsEmpty));
            }
        }
    }

    public bool ShowItemsEmpty => SelectedGroup is not null && Items.Count == 0;
    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }
    public ObservableCollection<TestSummaryDto> CatalogTests { get => _catalogTests; private set => SetProperty(ref _catalogTests, value); }
    public TestSummaryDto? PickedTest { get => _pickedTest; set => SetProperty(ref _pickedTest, value); }
    public decimal NewItemPrice { get => _newItemPrice; set => SetProperty(ref _newItemPrice, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadGroupsCommand { get; }
    public RelayCommand NewCommand { get; }
    public AsyncRelayCommand SaveGroupCommand { get; }
    public AsyncRelayCommand DeleteGroupCommand { get; }
    public AsyncRelayCommand AddItemCommand { get; }
    public AsyncRelayCommand SaveItemPriceCommand { get; }
    public AsyncRelayCommand RemoveItemCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var groupsResult = await _mediator.Send(new GetCustomGroupsQuery());
            if (groupsResult.IsSuccess && groupsResult.Value is not null)
            {
                Groups = new ObservableCollection<CustomGroupSummaryDto>(groupsResult.Value);
            }
            else if (groupsResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(groupsResult.Error);
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
        SelectedGroup = null;
        GroupName = string.Empty;
        Items = new ObservableCollection<CustomGroupItemRow>();
        OnPropertyChanged(nameof(ShowItemsEmpty));
    }

    private async Task ReloadItemsAsync()
    {
        OnPropertyChanged(nameof(ShowItemsEmpty));
        if (SelectedGroup is null)
        {
            Items = new ObservableCollection<CustomGroupItemRow>();
            return;
        }

        var result = await _mediator.Send(new GetCustomGroupByIdQuery(SelectedGroup.Id));
        if (result.IsSuccess && result.Value is not null)
        {
            Items = new ObservableCollection<CustomGroupItemRow>(
                result.Value.Items.Select(i => new CustomGroupItemRow(i)
                {
                    IsSaved = i.TestId == _lastSavedTestId
                }));
        }
        else if (result.Error is not null)
        {
            ErrorMessage = _presenter.Present(result.Error);
        }
    }

    public async Task SaveGroupAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (SelectedGroup is null)
            {
                var result = await _mediator.Send(new CreateCustomGroupCommand(GroupName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم إنشاء المجموعة.";
            }
            else
            {
                var result = await _mediator.Send(new RenameCustomGroupCommand(SelectedGroup.Id, GroupName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ المجموعة.";
            }

            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "حذف المجموعة",
            $"سيتم حذف المجموعة \"{SelectedGroup.Name}\" — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeleteCustomGroupCommand(SelectedGroup.Id));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        StatusMessage = "تم حذف المجموعة.";
        ClearEditor();
        await LoadAsync();
    }

    private async Task AddItemAsync()
    {
        if (SelectedGroup is null)
        {
            ErrorMessage = "اختر مجموعة أولاً.";
            return;
        }

        if (PickedTest is null)
        {
            ErrorMessage = "اختر تحليلاً لإضافته.";
            return;
        }

        if (Items.Any(i => i.Item.TestId == PickedTest.Id))
        {
            ErrorMessage = "التحليل مضاف لهذه المجموعة بالفعل";
            return;
        }

        var result = await _mediator.Send(new SetCustomGroupItemPriceCommand(
            SelectedGroup.Id, PickedTest.Id, NewItemPrice));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        _lastSavedTestId = PickedTest.Id;
        await ReloadItemsAsync();
        await LoadAsync();
    }

    private async Task SaveItemPriceAsync(CustomGroupItemRow? row)
    {
        if (SelectedGroup is null || row is null)
        {
            return;
        }

        var result = await _mediator.Send(new SetCustomGroupItemPriceCommand(
            SelectedGroup.Id, row.Item.TestId, row.EditPrice));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        _lastSavedTestId = row.Item.TestId;
        await ReloadItemsAsync();
    }

    private async Task RemoveItemAsync(CustomGroupItemRow? row)
    {
        if (SelectedGroup is null || row is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "إزالة البند",
            $"سيتم إزالة التحليل \"{row.Item.TestName}\" من المجموعة — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new RemoveCustomGroupItemCommand(SelectedGroup.Id, row.Item.TestId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        await ReloadItemsAsync();
        await LoadAsync();
    }
}

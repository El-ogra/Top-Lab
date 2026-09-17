using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceLists;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class PriceListItemRow : ViewModelBase
{
    private decimal _editPrice;
    private bool _isSaved;

    public PriceListItemRow(PriceListItemDto item)
    {
        Item = item;
        _editPrice = item.Price;
    }

    public PriceListItemDto Item { get; }

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
/// Price-lists master-detail tab (S-02 Slice 7): row-by-row price save with
/// a «محفوظ» badge. No print control exists (unresolved owner decision).
/// </summary>
public sealed class PriceListsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<PriceListSummaryDto> _lists = new();
    private PriceListSummaryDto? _selectedList;
    private ObservableCollection<PriceListItemRow> _items = new();
    private string _listName = string.Empty;
    private ObservableCollection<TestSummaryDto> _catalogTests = new();
    private TestSummaryDto? _pickedTest;
    private decimal _newItemPrice;
    private int _lastSavedTestId;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public PriceListsViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        LoadListsCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => ClearEditor());
        SaveListCommand = new AsyncRelayCommand(_ => SaveListAsync());
        DeleteListCommand = new AsyncRelayCommand(_ => DeleteListAsync());
        AddItemCommand = new AsyncRelayCommand(_ => AddItemAsync());
        SaveItemPriceCommand = new AsyncRelayCommand((p, _) => SaveItemPriceAsync(p as PriceListItemRow));
        RemoveItemCommand = new AsyncRelayCommand((p, _) => RemoveItemAsync(p as PriceListItemRow));
    }

    public ObservableCollection<PriceListSummaryDto> Lists { get => _lists; private set => SetProperty(ref _lists, value); }

    public PriceListSummaryDto? SelectedList
    {
        get => _selectedList;
        set
        {
            if (SetProperty(ref _selectedList, value))
            {
                ListName = value?.Name ?? string.Empty;
                _ = ReloadItemsAsync();
            }
        }
    }

    public ObservableCollection<PriceListItemRow> Items
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

    public bool ShowItemsEmpty => SelectedList is not null && Items.Count == 0;
    public string ListName { get => _listName; set => SetProperty(ref _listName, value); }
    public ObservableCollection<TestSummaryDto> CatalogTests { get => _catalogTests; private set => SetProperty(ref _catalogTests, value); }
    public TestSummaryDto? PickedTest { get => _pickedTest; set => SetProperty(ref _pickedTest, value); }
    public decimal NewItemPrice { get => _newItemPrice; set => SetProperty(ref _newItemPrice, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand LoadListsCommand { get; }
    public RelayCommand NewCommand { get; }
    public AsyncRelayCommand SaveListCommand { get; }
    public AsyncRelayCommand DeleteListCommand { get; }
    public AsyncRelayCommand AddItemCommand { get; }
    public AsyncRelayCommand SaveItemPriceCommand { get; }
    public AsyncRelayCommand RemoveItemCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var listsResult = await _mediator.Send(new GetPriceListsQuery());
            if (listsResult.IsSuccess && listsResult.Value is not null)
            {
                Lists = new ObservableCollection<PriceListSummaryDto>(listsResult.Value);
            }
            else if (listsResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(listsResult.Error);
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
        SelectedList = null;
        ListName = string.Empty;
        Items = new ObservableCollection<PriceListItemRow>();
        OnPropertyChanged(nameof(ShowItemsEmpty));
    }

    private async Task ReloadItemsAsync()
    {
        OnPropertyChanged(nameof(ShowItemsEmpty));
        if (SelectedList is null)
        {
            Items = new ObservableCollection<PriceListItemRow>();
            return;
        }

        var result = await _mediator.Send(new GetPriceListByIdQuery(SelectedList.Id));
        if (result.IsSuccess && result.Value is not null)
        {
            Items = new ObservableCollection<PriceListItemRow>(
                result.Value.Items.Select(i => new PriceListItemRow(i)
                {
                    IsSaved = i.TestId == _lastSavedTestId
                }));
        }
        else if (result.Error is not null)
        {
            ErrorMessage = _presenter.Present(result.Error);
        }
    }

    public async Task SaveListAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (SelectedList is null)
            {
                var result = await _mediator.Send(new CreatePriceListCommand(ListName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم إنشاء القائمة.";
            }
            else
            {
                var result = await _mediator.Send(new RenamePriceListCommand(SelectedList.Id, ListName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ القائمة.";
            }

            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteListAsync()
    {
        if (SelectedList is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "حذف القائمة",
            $"سيتم حذف قائمة الأسعار \"{SelectedList.Name}\" — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeletePriceListCommand(SelectedList.Id));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        StatusMessage = "تم حذف القائمة.";
        ClearEditor();
        await LoadAsync();
    }

    private async Task AddItemAsync()
    {
        if (SelectedList is null)
        {
            ErrorMessage = "اختر قائمة أولاً.";
            return;
        }

        if (PickedTest is null)
        {
            ErrorMessage = "اختر تحليلاً لإضافته.";
            return;
        }

        if (Items.Any(i => i.Item.TestId == PickedTest.Id))
        {
            ErrorMessage = "هذا التحليل مسعَّر في هذه القائمة بالفعل";
            return;
        }

        var result = await _mediator.Send(new SetPriceListItemPriceCommand(
            SelectedList.Id, PickedTest.Id, NewItemPrice));
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

    private async Task SaveItemPriceAsync(PriceListItemRow? row)
    {
        if (SelectedList is null || row is null)
        {
            return;
        }

        var result = await _mediator.Send(new SetPriceListItemPriceCommand(
            SelectedList.Id, row.Item.TestId, row.EditPrice));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        _lastSavedTestId = row.Item.TestId;
        await ReloadItemsAsync();
    }

    private async Task RemoveItemAsync(PriceListItemRow? row)
    {
        if (SelectedList is null || row is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "إزالة البند",
            $"سيتم إزالة التحليل \"{row.Item.TestName}\" من القائمة — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new RemovePriceListItemCommand(SelectedList.Id, row.Item.TestId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        await ReloadItemsAsync();
        await LoadAsync();
    }
}

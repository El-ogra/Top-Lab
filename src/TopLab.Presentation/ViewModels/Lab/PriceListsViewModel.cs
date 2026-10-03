using System.Collections.ObjectModel;
using System.IO;
using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
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
/// a «محفوظ» badge.
///
/// P-01 F8 (PP-03): printing added. It goes through the Application-layer
/// <see cref="IPriceListPdfWriter"/> port, so this ViewModel never names an
/// Infrastructure type (SD-10). The command is disabled with no selection —
/// no half-wired state.
/// </summary>
public sealed class PriceListsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private readonly IPriceListPdfWriter _priceListPdfWriter;
    private readonly ILabPrintTextStore _labPrintTextStore;

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
        ResultErrorPresenter presenter,
        IPriceListPdfWriter priceListPdfWriter,
        ILabPrintTextStore labPrintTextStore)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;
        _priceListPdfWriter = priceListPdfWriter;
        _labPrintTextStore = labPrintTextStore;

        LoadListsCommand = new AsyncRelayCommand(_ => LoadAsync());
        NewCommand = new RelayCommand(_ => ClearEditor());
        SaveListCommand = new AsyncRelayCommand(_ => SaveListAsync());
        DeleteListCommand = new AsyncRelayCommand(_ => DeleteListAsync());
        AddItemCommand = new AsyncRelayCommand(_ => AddItemAsync());
        SaveItemPriceCommand = new AsyncRelayCommand((p, _) => SaveItemPriceAsync(p as PriceListItemRow));
        RemoveItemCommand = new AsyncRelayCommand((p, _) => RemoveItemAsync(p as PriceListItemRow));
        // P-01 F8: enabled only when a list is selected — no half-wired state.
        PrintListCommand = new AsyncRelayCommand(_ => PrintListAsync(), () => SelectedList is not null);
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
                // P-01 F8: keep the print command's enabled state in step with the selection.
                PrintListCommand.RaiseCanExecuteChanged();
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

    /// <summary>P-01 F8 — print the selected price list as a PDF.</summary>
    public AsyncRelayCommand PrintListCommand { get; }

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

    private async Task PrintListAsync()
    {
    if (SelectedList is null)
    {
        return;
    }

    IsBusy = true;
    ErrorMessage = string.Empty;
    try
    {
        var detailResult = await _mediator.Send(new GetPriceListByIdQuery(SelectedList.Id));
        if (!detailResult.IsSuccess || detailResult.Value is null)
        {
            ErrorMessage = detailResult.Error is not null
                ? _presenter.Present(detailResult.Error)
                : "تعذّر تحميل قائمة الأسعار.";
            return;
        }

        var path = await _dialogs.PickPdfSavePathAsync($"{SelectedList.Name}.pdf");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var labTextResult = await _labPrintTextStore.GetAsync(LabPrintTextScope.Report);
        if (!labTextResult.IsSuccess || labTextResult.Value is null)
        {
            ErrorMessage = labTextResult.Error is not null
                ? _presenter.Present(labTextResult.Error)
                : "تعذّر تحميل بيانات المعمل للطباعة.";
            return;
        }

        await _priceListPdfWriter.WritePdfAsync(path, detailResult.Value, labTextResult.Value);
        StatusMessage = "تم إنشاء ملف قائمة الأسعار.";
    }
    catch (IOException)
    {
        ErrorMessage = "الملف موجود مسبقًا؛ لم يتم الكتابة فوقه.";
    }
    catch (UnauthorizedAccessException)
    {
        ErrorMessage = "لا توجد صلاحية للكتابة في المسار المحدد.";
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        ErrorMessage = _presenter.Present(Error.Unexpected(ex.Message));
    }
    finally
    {
        IsBusy = false;
    }
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

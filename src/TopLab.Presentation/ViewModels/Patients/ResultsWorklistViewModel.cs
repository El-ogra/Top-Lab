using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 2: Results Worklist (R1) — unified worklist showing all result kinds
/// (simple, specialized profile, culture) from GetResultWorklistQuery.
/// Routing D4: Simple → R2 (S3), SpecializedProfile → P1 (S6), Culture → C1 (S7).
/// </summary>
public sealed class ResultsWorklistViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private DateOnly? _day = DateOnly.FromDateTime(DateTime.UtcNow);
    private bool? _hasResult = null;
    private bool? _isReviewed = null;
    private int? _testGroupId;
    private int? _resultKind;
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<ResultWorklistItemDto> _items = new();
    private int _totalCount;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private ResultWorklistItemDto? _selectedItem;

    public ResultsWorklistViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        OpenDetailCommand = new AsyncRelayCommand(async (param, ct) => await OpenDetailAsync(param as ResultWorklistItemDto, ct));
    }

    public DateOnly? Day
    {
        get => _day;
        set => SetProperty(ref _day, value);
    }

    public bool? HasResult
    {
        get => _hasResult;
        set
        {
            if (SetProperty(ref _hasResult, value))
            {
                Page = 1;
            }
        }
    }

    public bool? IsReviewed
    {
        get => _isReviewed;
        set
        {
            if (SetProperty(ref _isReviewed, value))
            {
                Page = 1;
            }
        }
    }

    public int? TestGroupId
    {
        get => _testGroupId;
        set => SetProperty(ref _testGroupId, value);
    }

    public int? ResultKind
    {
        get => _resultKind;
        set
        {
            if (SetProperty(ref _resultKind, value))
            {
                Page = 1;
            }
        }
    }

    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    public ResultWorklistItemDto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ObservableCollection<ResultWorklistItemDto> Items
    {
        get => _items;
        private set
        {
            if (SetProperty(ref _items, value))
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public bool HasResults => Items.Count > 0;
    public bool ShowEmpty => Items.Count == 0;

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand OpenDetailCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetResultWorklistQuery(
                Day, HasResult, IsReviewed, TestGroupId, ResultKind, Page, PageSize), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                Items = new ObservableCollection<ResultWorklistItemDto>(result.Value);
                TotalCount = result.Value.Count;
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenDetailAsync(ResultWorklistItemDto? item, CancellationToken cancellationToken)
    {
        if (item is null)
        {
            return;
        }

        // S-04 Slice 3: Simple results route to SimpleResultEntryViewModel (enabled)
        // D4 routing: Simple tests → R2 (S3), SpecializedProfile → P1 (S6), Culture → C1 (S7)
        // Note: item.ResultKind is int, ResultKind enum values: Simple=0, SpecializedProfile=1, Culture=2
        if (item.ResultKind == 0)  // ResultKind.Simple = 0
        {
            _navigation.NavigateTo<SimpleResultEntryViewModel>();
            if (_navigation.CurrentViewModel is SimpleResultEntryViewModel vm)
            {
                await vm.LoadAsync(item);
            }
        }
        // S-04 Slice 6: SpecializedProfile routes to ProfileEntryViewModel (P1)
        else if (item.ResultKind == 1)  // ResultKind.SpecializedProfile = 1
        {
            _navigation.NavigateTo<ProfileEntryViewModel>();
            if (_navigation.CurrentViewModel is ProfileEntryViewModel pvm)
            {
                await pvm.LoadAsync(item.PatientTestId, cancellationToken);
            }
        }
        // S-04 Slice 7: Culture routes to CultureResultsViewModel (C1) - deferred
    }
}
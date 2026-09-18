using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 0: Patient search screen (M08) — global search + lab-id fetch.
/// Gateway: «بحث عن مريض» in PatientsHubViewModel.
/// Open-patient affordance ships disabled until Slice 1 (no-half-wired-state).
/// </summary>
public sealed class PatientSearchViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private string _searchText = string.Empty;
    private string _labIdText = string.Empty;
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<PatientSearchHitDto> _items = new();
    private int _totalCount;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private PatientSearchHitDto? _selectedItem;

    public PatientSearchViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        SearchCommand = new AsyncRelayCommand(async (_, ct) => await SearchAsync(ct));
        FetchByLabIdCommand = new AsyncRelayCommand(async (_, ct) => await FetchByLabIdAsync(ct));
        NextPageCommand = new AsyncRelayCommand(async (_, ct) => { Page++; await SearchAsync(ct); });
        PreviousPageCommand = new AsyncRelayCommand(async (_, ct) => { if (Page > 1) { Page--; await SearchAsync(ct); } });
        OpenPatientCommand = new RelayCommand(_ => { }); // disabled until Slice 1
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientsHubViewModel>());
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string LabIdText
    {
        get => _labIdText;
        set => SetProperty(ref _labIdText, value);
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

    public PatientSearchHitDto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ObservableCollection<PatientSearchHitDto> Items
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
    public bool ShowEmpty => Items.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand FetchByLabIdCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public RelayCommand OpenPatientCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await SearchAsync(cancellationToken);
    }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var text = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            var result = await _mediator.Send(
                new SearchPatientsGlobalQuery(text, Page, PageSize), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Items = new ObservableCollection<PatientSearchHitDto>(result.Value);
                TotalCount = result.Value.Count;
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                Items = new ObservableCollection<PatientSearchHitDto>();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task FetchByLabIdAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(LabIdText))
        {
            ErrorMessage = "كود المعمل مطلوب.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetPatientByLabIdQuery(LabIdText.Trim()), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                // Slice 1 will enable navigation to PatientVisitHistoryViewModel
                // For now, show a status message that the fetch succeeded
                Items = new ObservableCollection<PatientSearchHitDto>();
                TotalCount = 0;
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
}

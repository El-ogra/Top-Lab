using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 4: Result delivery list screen (M09) — undelivered patients by period.
/// Gateway: «تسليم نتائج المرضى» in PatientsHubViewModel.
/// </summary>
public sealed class ResultDeliveryViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private DateOnly? _from = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly? _to = DateOnly.FromDateTime(DateTime.UtcNow);
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<UndeliveredPatientRowDto> _items = new();
    private UndeliveredPatientRowDto? _selectedItem;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public ResultDeliveryViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        OpenHandoverCommand = new RelayCommand(param => OpenHandover(param as UndeliveredPatientRowDto));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientsHubViewModel>());
    }

    public DateOnly? From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }

    public DateOnly? To
    {
        get => _to;
        set => SetProperty(ref _to, value);
    }

    public int Page { get => _page; set => SetProperty(ref _page, value); }
    public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }

    public UndeliveredPatientRowDto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ObservableCollection<UndeliveredPatientRowDto> Items
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

    public AsyncRelayCommand LoadCommand { get; }
    public RelayCommand OpenHandoverCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetUndeliveredResultsQuery(From, To, Page, PageSize), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Items = new ObservableCollection<UndeliveredPatientRowDto>(result.Value);
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                Items = new ObservableCollection<UndeliveredPatientRowDto>();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenHandover(UndeliveredPatientRowDto? row)
    {
        if (row is null || row.PatientId <= 0)
        {
            return;
        }

        _navigation.NavigateTo<DeliveryHandoverViewModel>();
        if (_navigation.CurrentViewModel is DeliveryHandoverViewModel vm)
        {
            _ = vm.LoadAsync(row.PatientId);
        }
    }
}

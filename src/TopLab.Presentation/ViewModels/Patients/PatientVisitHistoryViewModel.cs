using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Application.Features.PatientSearch.Queries.GetVisitDetail;
using TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 1: Patient visit history master-detail (M08).
/// Cross-module buttons: «فتح الحساب» → PatientAccountViewModel (enabled);
/// «كشف النتائج» → PatientResultSheetViewModel (enabled);
/// «التقارير»/«التسليم»/«إرسال خارجياً» ship disabled until their slices land.
/// </summary>
public sealed class PatientVisitHistoryViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private int _patientId;
    private VisitHistoryDto? _history;
    private VisitDetailDto? _selectedVisitDetail;
    private VisitSummaryDto? _selectedVisit;
    private ObservableCollection<VisitSummaryDto> _visits = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public PatientVisitHistoryViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        OpenAccountCommand = new RelayCommand(_ => OpenAccount());
        OpenResultSheetCommand = new RelayCommand(_ => OpenResultSheet());
        OpenReportsCommand = new RelayCommand(_ => OpenReports());
        OpenDeliveryCommand = new RelayCommand(_ => OpenDelivery());
        OpenSentOutCommand = new RelayCommand(_ => OpenSentOut());
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientSearchViewModel>());
    }

    public int PatientId => _patientId;
    public string PatientFullName => _history?.PatientFullName ?? string.Empty;
    public string LabId => _history?.LabId ?? string.Empty;
    public string HistorySortMode => _history?.HistorySortMode ?? string.Empty;
    public bool HistoryAutoDisplayEnabled => _history?.HistoryAutoDisplayEnabled ?? false;

    public VisitDetailDto? SelectedVisitDetail
    {
        get => _selectedVisitDetail;
        private set
        {
            if (SetProperty(ref _selectedVisitDetail, value))
            {
                OnPropertyChanged(nameof(HasVisitDetail));
                OnPropertyChanged(nameof(ShowVisitEmpty));
            }
        }
    }

    public bool HasVisitDetail => SelectedVisitDetail is not null;
    public bool ShowVisitEmpty => SelectedVisitDetail is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public VisitSummaryDto? SelectedVisit
    {
        get => _selectedVisit;
        set
        {
            if (SetProperty(ref _selectedVisit, value) && value is not null)
            {
                _ = LoadVisitDetailAsync(value.PatientId);
            }
        }
    }

    public ObservableCollection<VisitSummaryDto> Visits
    {
        get => _visits;
        private set
        {
            if (SetProperty(ref _visits, value))
            {
                OnPropertyChanged(nameof(HasVisits));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasVisits => Visits.Count > 0;
    public bool ShowEmpty => Visits.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public RelayCommand OpenAccountCommand { get; }
    public RelayCommand OpenResultSheetCommand { get; }
    public RelayCommand OpenReportsCommand { get; }
    public RelayCommand OpenDeliveryCommand { get; }
    public RelayCommand OpenSentOutCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        IsBusy = true;
        ErrorMessage = string.Empty;
        SelectedVisitDetail = null;

        try
        {
            var historyResult = await _mediator.Send(new GetVisitHistoryQuery(patientId), cancellationToken);
            if (historyResult.IsSuccess && historyResult.Value is not null)
            {
                _history = historyResult.Value;
                OnPropertyChanged(nameof(PatientFullName));
                OnPropertyChanged(nameof(LabId));
                OnPropertyChanged(nameof(HistorySortMode));
                OnPropertyChanged(nameof(HistoryAutoDisplayEnabled));

                Visits = new ObservableCollection<VisitSummaryDto>(_history.Visits);

                // Auto-load detail for the first visit
                if (Visits.Count > 0)
                {
                    SelectedVisit = Visits[0];
                }
            }
            else if (historyResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(historyResult.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Load with a pre-fetched VisitHistoryDto (from lab-id fetch path).</summary>
    public async Task LoadWithHistoryAsync(int patientId, VisitHistoryDto history, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        _history = history;
        IsBusy = true;
        ErrorMessage = string.Empty;
        SelectedVisitDetail = null;

        OnPropertyChanged(nameof(PatientFullName));
        OnPropertyChanged(nameof(LabId));
        OnPropertyChanged(nameof(HistorySortMode));
        OnPropertyChanged(nameof(HistoryAutoDisplayEnabled));

        Visits = new ObservableCollection<VisitSummaryDto>(history.Visits);

        if (Visits.Count > 0)
        {
            SelectedVisit = Visits[0];
        }

        IsBusy = false;
        await Task.CompletedTask;
    }

    private async Task LoadVisitDetailAsync(int patientId, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var detailResult = await _mediator.Send(new GetVisitDetailQuery(patientId), cancellationToken);
            if (detailResult.IsSuccess && detailResult.Value is not null)
            {
                SelectedVisitDetail = detailResult.Value;
            }
            else if (detailResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(detailResult.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenAccount()
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرف المريض غير صالح.";
            return;
        }

        _navigation.NavigateTo<PatientAccountViewModel>();
        if (_navigation.CurrentViewModel is PatientAccountViewModel vm)
        {
            _ = vm.LoadAsync(_patientId);
        }
    }

    private void OpenResultSheet()
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرف المريض غير صالح.";
            return;
        }

        _navigation.NavigateTo<PatientResultSheetViewModel>();
        if (_navigation.CurrentViewModel is PatientResultSheetViewModel vm)
        {
            _ = vm.LoadAsync(_patientId);
        }
    }

    private void OpenReports()
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرف المريض غير صالح.";
            return;
        }

        _navigation.NavigateTo<CombinedReportViewModel>();
        if (_navigation.CurrentViewModel is CombinedReportViewModel vm)
        {
            _ = vm.LoadAsync(_patientId);
        }
    }

    private void OpenDelivery()
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرف المريض غير صالح.";
            return;
        }

        _navigation.NavigateTo<DeliveryHandoverViewModel>();
        if (_navigation.CurrentViewModel is DeliveryHandoverViewModel vm)
        {
            _ = vm.LoadAsync(_patientId);
        }
    }

    private void OpenSentOut()
    {
        _navigation.NavigateTo<SentOutSamplesViewModel>();
        if (_navigation.CurrentViewModel is SentOutSamplesViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}

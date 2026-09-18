using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Audit;

/// <summary>
/// S-06 Slice 5: Audit screen (M10) — P/T tabs.
/// D12 closed: «النظام» shell title navigates directly to AuditViewModel.
/// </summary>
public sealed class AuditViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private int _selectedTab; // 0=P (patient), 1=T (test)
    private int _patientIdInput;
    private int _patientTestIdInput;
    private PatientAuditDto? _patientAudit;
    private PatientTestAuditDto? _testAudit;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public AuditViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadPatientAuditCommand = new AsyncRelayCommand(async (_, ct) => await LoadPatientAuditAsync(ct));
        LoadTestAuditCommand = new AsyncRelayCommand(async (_, ct) => await LoadTestAuditAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Shell.HomeViewModel>());
    }

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                OnPropertyChanged(nameof(IsPatientTab));
                OnPropertyChanged(nameof(IsTestTab));
            }
        }
    }

    public bool IsPatientTab => SelectedTab == 0;
    public bool IsTestTab => SelectedTab == 1;

    public int PatientIdInput { get => _patientIdInput; set => SetProperty(ref _patientIdInput, value); }
    public int PatientTestIdInput { get => _patientTestIdInput; set => SetProperty(ref _patientTestIdInput, value); }

    public PatientAuditDto? PatientAudit
    {
        get => _patientAudit;
        private set
        {
            if (SetProperty(ref _patientAudit, value))
            {
                OnPropertyChanged(nameof(HasPatientAudit));
                OnPropertyChanged(nameof(ShowPatientEmpty));
            }
        }
    }

    public bool HasPatientAudit => _patientAudit is not null;
    public bool ShowPatientEmpty => _patientAudit is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public PatientTestAuditDto? TestAudit
    {
        get => _testAudit;
        private set
        {
            if (SetProperty(ref _testAudit, value))
            {
                OnPropertyChanged(nameof(HasTestAudit));
                OnPropertyChanged(nameof(ShowTestEmpty));
            }
        }
    }

    public bool HasTestAudit => _testAudit is not null;
    public bool ShowTestEmpty => _testAudit is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand LoadPatientAuditCommand { get; }
    public AsyncRelayCommand LoadTestAuditCommand { get; }
    public RelayCommand BackCommand { get; }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>Contextual entry: fill PatientId from M08 visit history.</summary>
    public async Task LoadWithPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        PatientIdInput = patientId;
        SelectedTab = 0;
        await LoadPatientAuditAsync(cancellationToken);
    }

    /// <summary>Contextual entry: fill PatientTestId from M04 result screens.</summary>
    public async Task LoadWithPatientTestIdAsync(int patientTestId, CancellationToken cancellationToken = default)
    {
        PatientTestIdInput = patientTestId;
        SelectedTab = 1;
        await LoadTestAuditAsync(cancellationToken);
    }

    private async Task LoadPatientAuditAsync(CancellationToken cancellationToken)
    {
        if (PatientIdInput <= 0)
        {
            ErrorMessage = "معرف غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        PatientAudit = null;

        try
        {
            var result = await _mediator.Send(new GetPatientAuditQuery(PatientIdInput), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                PatientAudit = result.Value;
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

    private async Task LoadTestAuditAsync(CancellationToken cancellationToken)
    {
        if (PatientTestIdInput <= 0)
        {
            ErrorMessage = "معرف غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        TestAudit = null;

        try
        {
            var result = await _mediator.Send(new GetPatientTestAuditQuery(PatientTestIdInput), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                TestAudit = result.Value;
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

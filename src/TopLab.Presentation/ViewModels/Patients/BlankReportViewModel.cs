using MediatR;
using TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;
using TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 3: Blank report screen (M07) — build + print blank report.
/// No balance gate, no MarkPrinted (OD-07-E backend-settled).
/// </summary>
public sealed class BlankReportViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private int _patientId;
    private BlankReportDto? _report;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public BlankReportViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        BuildCommand = new AsyncRelayCommand(async (_, ct) => await BuildAsync(ct));
        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientVisitHistoryViewModel>());
    }

    public int PatientId => _patientId;

    public BlankReportDto? Report
    {
        get => _report;
        private set
        {
            if (SetProperty(ref _report, value))
            {
                OnPropertyChanged(nameof(HasReport));
                OnPropertyChanged(nameof(ShowNotBuilt));
            }
        }
    }

    public bool HasReport => _report is not null;
    public bool ShowNotBuilt => _report is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand BuildCommand { get; }
    public AsyncRelayCommand PrintCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        Report = null;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        await Task.CompletedTask;
    }

    private async Task BuildAsync(CancellationToken cancellationToken)
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرّف المريض غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new BuildBlankReportCommand(_patientId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                Report = result.Value;
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

    private async Task PrintAsync(CancellationToken cancellationToken)
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرّف المريض غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new PrintBlankReportCommand(_patientId), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تمت الطباعة.";
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

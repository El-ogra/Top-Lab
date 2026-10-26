using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Clipboard;
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
    private readonly IClipboardService _clipboard;

    private DateOnly? _from = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly? _to = DateOnly.FromDateTime(DateTime.UtcNow);
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<UndeliveredPatientRowDto> _items = new();
    private UndeliveredPatientRowDto? _selectedItem;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public ResultDeliveryViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        INavigationService navigation,
        IClipboardService clipboard)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;
        _clipboard = clipboard;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        OpenHandoverCommand = new RelayCommand(param => OpenHandover(param as UndeliveredPatientRowDto));
        OpenGroupSlipsCommand = new RelayCommand(_ => OpenGroupSlips());
        PatientAccountCommand = new RelayCommand(param => OpenPatientAccount(param as UndeliveredPatientRowDto), HasRow);
        CombinedReportCommand = new RelayCommand(param => OpenCombinedReport(param as UndeliveredPatientRowDto), HasRow);
        EmptyReportCommand = new RelayCommand(param => OpenEmptyReport(param as UndeliveredPatientRowDto), HasRow);
        CopyCommand = new RelayCommand(param => CopyRow(param as UndeliveredPatientRowDto), HasRow);
        PatientDataCommand = new RelayCommand(param => OpenPatientData(param as UndeliveredPatientRowDto), HasRow);
        PrintEnvelopeCommand = new AsyncRelayCommand(async (param, ct) => await PrintEnvelopeAsync(param as UndeliveredPatientRowDto, ct), HasRow);
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

    /// <summary>
    /// The grid's real selection (ResultDeliveryView.xaml binds <c>DataGrid.SelectedItem</c>
    /// to this property; the three account/report buttons pass it as
    /// <c>CommandParameter</c>). The setter re-evaluates the row-gated commands so the
    /// buttons disable while no row is selected (R1 Round 3 A2).
    /// </summary>
    public UndeliveredPatientRowDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                PatientAccountCommand.RaiseCanExecuteChanged();
                CombinedReportCommand.RaiseCanExecuteChanged();
                EmptyReportCommand.RaiseCanExecuteChanged();
                CopyCommand.RaiseCanExecuteChanged();
                PatientDataCommand.RaiseCanExecuteChanged();
                PrintEnvelopeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Row gate for the three account/report buttons: a selectable row must exist.</summary>
    private static bool HasRow(object? parameter)
        => parameter is UndeliveredPatientRowDto row && row.PatientId > 0;

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
    public RelayCommand OpenGroupSlipsCommand { get; }
    public RelayCommand PatientAccountCommand { get; }
    public RelayCommand CombinedReportCommand { get; }
    public RelayCommand EmptyReportCommand { get; }
    /// <summary>1.4.1.4.5 «Copy»: copies the selected row's textual summary to the clipboard.</summary>
    public RelayCommand CopyCommand { get; }

    /// <summary>1.4.1.4.7 «بيانات المريض»: opens the patient's data from the delivery screen.</summary>
    public RelayCommand PatientDataCommand { get; }

    /// <summary>
    /// 1.4.1.4.11 «طباعة ظرف»: prints the result envelope for the selected patient, reusing the
    /// existing <c>PrintEnvelopeCommand</c> the patient editor and result sheet already use.
    /// </summary>
    public AsyncRelayCommand PrintEnvelopeCommand { get; }

    public RelayCommand BackCommand { get; }

    /// <summary>1.4.1.4.2 «Refresh»: reloads the delivery list for the current period.</summary>
    public AsyncRelayCommand RefreshCommand => LoadCommand;

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

    /// <summary>
    /// 1.4.1.4.5 «Copy»: copies a plain-text summary of the selected delivery entry.
    /// The summary is built here (not in the view) so a test can assert its exact content.
    /// </summary>
    private void CopyRow(UndeliveredPatientRowDto? row)
    {
        if (row is null || row.PatientId <= 0)
        {
            return;
        }

        _ = _clipboard.SetTextAsync(BuildCopySummary(row));
    }

    /// <summary>
    /// The exact text the copy action puts on the clipboard: patient, lab id, registration
    /// date, visit status and the number of undelivered results, plus the note when present.
    /// </summary>
    public static string BuildCopySummary(UndeliveredPatientRowDto row)
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("المريض: ").Append(row.PatientFullName);
        builder.Append(" — Lab ID: ").Append(row.LabId ?? "—");
        builder.Append(" — التاريخ: ").Append(row.RegistrationDateUtc.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
        builder.Append(" — الحالة: ").Append(row.VisitStatus);
        builder.Append(" — غير المسلَّم: ").Append(row.UndeliveredCount);

        if (!string.IsNullOrWhiteSpace(row.PatientNotes))
        {
            builder.Append(" — ملاحظات: ").Append(row.PatientNotes);
        }

        return builder.ToString();
    }

    /// <summary>
    /// 1.4.1.4.7 «بيانات المريض»: opens the patient editor for the selected patient, carrying
    /// the patient id the same way the account action does.
    /// </summary>
    private void OpenPatientData(UndeliveredPatientRowDto? row)
    {
        if (row is null || row.PatientId <= 0)
        {
            return;
        }

        _navigation.NavigateTo<PatientEditorViewModel>();
        if (_navigation.CurrentViewModel is PatientEditorViewModel vm)
        {
            _ = vm.LoadPatientAsync(row.PatientId);
        }
    }

    /// <summary>1.4.1.4.11: the result envelope for one patient, through the existing handler.</summary>
    private async Task PrintEnvelopeAsync(UndeliveredPatientRowDto? row, CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        if (row is null || row.PatientId <= 0)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new PrintEnvelopeCommand(row.PatientId), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
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

    /// <summary>
    /// Phase 6 (GAP-013): opens the group-patient report slip screen, carrying the
    /// period the user is currently viewing so the patient list starts consistent
    /// with the delivery list behind it.
    /// </summary>
    private void OpenGroupSlips()
    {
        _navigation.NavigateTo<GroupPatientSlipsViewModel>();
        if (_navigation.CurrentViewModel is GroupPatientSlipsViewModel vm)
        {
            vm.From = From;
            vm.To = To;
            _ = vm.LoadAsync();
        }
    }

    private void OpenPatientAccount(UndeliveredPatientRowDto? row)
    {
        if (row is null || row.PatientId <= 0) return;
        _navigation.NavigateTo<PatientAccountViewModel>();
        if (_navigation.CurrentViewModel is PatientAccountViewModel vm)
        {
            _ = vm.LoadAsync(row.PatientId);
        }
    }

    private void OpenCombinedReport(UndeliveredPatientRowDto? row)
    {
        if (row is null || row.PatientId <= 0) return;
        _navigation.NavigateTo<CombinedReportViewModel>();
        if (_navigation.CurrentViewModel is CombinedReportViewModel vm)
        {
            _ = vm.LoadAsync(row.PatientId);
        }
    }

    private void OpenEmptyReport(UndeliveredPatientRowDto? row)
    {
        if (row is null || row.PatientId <= 0) return;
        _navigation.NavigateTo<BlankReportViewModel>();
        if (_navigation.CurrentViewModel is BlankReportViewModel vm)
        {
            _ = vm.LoadAsync(row.PatientId);
        }
    }
}

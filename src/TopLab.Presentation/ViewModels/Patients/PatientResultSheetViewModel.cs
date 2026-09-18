using System.Collections.ObjectModel;
using System.IO;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Features.ResultsEntry.Commands.ExportPatientReportPdf;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 4/5: Patient result sheet (R3) — shows all test results for a patient.
/// S-04 Slice 5 adds: ExportPdfCommand (R6) and BulkPrintCommand (R5).
/// </summary>
public sealed class PatientResultSheetViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;

    private PatientResultSheetDto? _patientSheet;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public PatientResultSheetViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IServiceProvider services)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _services = services;

        LoadCommand = new AsyncRelayCommand(async (param, ct) =>
        {
            if (param is int patientId)
            {
                await LoadAsync(patientId, ct);
            }
        });

        ExportPdfCommand = new AsyncRelayCommand(async (_, ct) => await ExportPdfAsync(ct));
        BulkPrintCommand = new AsyncRelayCommand(async (_, ct) => await OpenBulkPrintAsync(ct));
    }

    public PatientResultSheetDto? PatientSheet
    {
        get => _patientSheet;
        private set
        {
            if (SetProperty(ref _patientSheet, value))
            {
                OnPropertyChanged(nameof(PatientId));
                OnPropertyChanged(nameof(PatientName));
                OnPropertyChanged(nameof(LabId));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public int PatientId => _patientSheet?.PatientId ?? 0;
    public string PatientName => _patientSheet?.PatientFullName ?? string.Empty;
    public string? LabId => _patientSheet?.LabId;
    public int TotalCount => _patientSheet?.Lines.Count ?? 0;

    public bool HasResults => _patientSheet?.Lines.Count > 0;
    public bool ShowEmpty => _patientSheet is null || _patientSheet.Lines.Count == 0;

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

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand ExportPdfCommand { get; }
    public AsyncRelayCommand BulkPrintCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetPatientResultSheetQuery(patientId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                PatientSheet = result.Value;
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

    /// <summary>R6: PDF export via SaveFileDialog → ExportPatientReportPdfCommand.</summary>
    private async Task ExportPdfAsync(CancellationToken cancellationToken)
    {
        if (PatientId == 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var suggestedName = $"{PatientName}_{LabId ?? "report"}.pdf";
        var path = await _dialogs.PickPdfSavePathAsync(suggestedName);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        // Enforce absolute + .pdf extension pre-call (backend also validates)
        if (!Path.IsPathFullyQualified(path))
        {
            ErrorMessage = "مسار ملف PDF يجب أن يكون مطلقًا.";
            return;
        }

        if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            path += ".pdf";
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new ExportPatientReportPdfCommand(PatientId, path), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم تصدير التقرير بنجاح.";
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

    /// <summary>R5: open bulk print dialog for the current patient.</summary>
    private async Task OpenBulkPrintAsync(CancellationToken cancellationToken)
    {
        if (PatientId == 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var vm = _services.GetRequiredService<BulkPrintDialogViewModel>();
        var window = new Views.Patients.BulkPrintDialogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        await vm.LoadPreflightAsync(new[] { PatientId }, cancellationToken);
        window.ShowDialog();

        // Refresh sheet after bulk print
        await LoadAsync(PatientId, cancellationToken);
    }
}

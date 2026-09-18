using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 4: Patient result sheet screen showing all test results for a patient
/// with reference ranges, flags, and notes. Displays patient summary and individual result lines.
/// </summary>
public sealed class PatientResultSheetViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private PatientResultSheetDto? _patientSheet;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public PatientResultSheetViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        LoadCommand = new AsyncRelayCommand(async (param, ct) => 
        {
            if (param is int patientId)
            {
                await LoadAsync(patientId, ct);
            }
        });
    }

    public PatientResultSheetDto? PatientSheet
    {
        get => _patientSheet;
        private set => SetProperty(ref _patientSheet, value);
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

    public AsyncRelayCommand LoadCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        await Task.CompletedTask; // placeholder for UI state
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
}
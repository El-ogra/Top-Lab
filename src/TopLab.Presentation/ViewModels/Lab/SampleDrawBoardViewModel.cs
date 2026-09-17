using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.SampleCollection.Commands.MarkAllSamplesDrawnForPatient;
using TopLab.Application.Features.SampleCollection.Commands.MarkSampleDrawn;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Application.Features.SampleCollection.Queries.GetPatientTestsForDraw;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

/// <summary>
/// S-04 Slice 1: patient draw board (S2). Modal dialog opened from the
/// Sample Collection worklist tab. Source = GetPatientTestsForDrawQuery →
/// SampleDrawBoardDto. Single mark → MarkSampleDrawnCommand (server clock
/// authoritative); bulk mark → MarkAllSamplesDrawnForPatientCommand behind
/// confirmation. Outside-lab rows' CheckBox disabled proactively.
/// </summary>
public sealed class SampleDrawBoardViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private int _patientId;
    private string _fullName = string.Empty;
    private string? _labId;
    private ObservableCollection<DrawRowViewModel> _notDrawn = new();
    private ObservableCollection<DrawRowViewModel> _drawn = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public SampleDrawBoardViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        MarkAllCommand = new AsyncRelayCommand(async _ => await MarkAllAsync());
        ReloadCommand = new AsyncRelayCommand(async _ => await LoadAsync());
    }

    public int PatientId
    {
        get => _patientId;
        private set => SetProperty(ref _patientId, value);
    }

    public string FullName
    {
        get => _fullName;
        private set => SetProperty(ref _fullName, value);
    }

    public string? LabId
    {
        get => _labId;
        private set => SetProperty(ref _labId, value);
    }

    public ObservableCollection<DrawRowViewModel> NotDrawnRows
    {
        get => _notDrawn;
        private set
        {
            if (SetProperty(ref _notDrawn, value))
            {
                OnPropertyChanged(nameof(AnyNotDrawn));
            }
        }
    }

    public ObservableCollection<DrawRowViewModel> DrawnRows
    {
        get => _drawn;
        private set
        {
            if (SetProperty(ref _drawn, value))
            {
                OnPropertyChanged(nameof(AnyDrawn));
            }
        }
    }

    public bool AnyNotDrawn => NotDrawnRows.Count > 0;
    public bool AnyDrawn => DrawnRows.Count > 0;

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

    public AsyncRelayCommand MarkAllCommand { get; }
    public AsyncRelayCommand ReloadCommand { get; }

    public async Task InitializeAsync(int patientId)
    {
        PatientId = patientId;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetPatientTestsForDrawQuery(PatientId));
            if (result.IsSuccess && result.Value is not null)
            {
                FullName = result.Value.FullName;
                LabId = result.Value.LabId;

                var notDrawn = new ObservableCollection<DrawRowViewModel>(
                    result.Value.NotDrawn.Select(r => new DrawRowViewModel(r, this)));
                var drawn = new ObservableCollection<DrawRowViewModel>(
                    result.Value.Drawn.Select(r => new DrawRowViewModel(r, this)));

                NotDrawnRows = notDrawn;
                DrawnRows = drawn;
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

    private async Task MarkAllAsync()
    {
        bool confirm = await _dialogs.ShowConfirmationAsync(
            "سحب الكل",
            "سحب كل العينات لهذا المريض؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new MarkAllSamplesDrawnForPatientCommand(PatientId));
            if (result.IsSuccess)
            {
                StatusMessage = $"تم سحب {result.Value} عينة.";
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                return;
            }

            await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal async Task MarkSampleAsync(PatientTestDrawDto row)
    {
        if (row.IsTakenOutsideLab)
        {
            ErrorMessage = "تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new MarkSampleDrawnCommand(
                row.PatientTestId, DateTime.MinValue));
            if (result.IsSuccess)
            {
                await LoadAsync();
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

/// <summary>
/// Wrapper for a single PatientTestDrawDto row on the draw board. The CheckBox
/// IsChecked → on set calls back to the parent's MarkSampleAsync. Outside-lab
/// rows disable the CheckBox proactively.
/// </summary>
public sealed class DrawRowViewModel : ViewModelBase
{
    private readonly SampleDrawBoardViewModel _parent;
    private bool _isChecked;

    public DrawRowViewModel(PatientTestDrawDto dto, SampleDrawBoardViewModel parent)
    {
        Dto = dto;
        _parent = parent;
    }

    public PatientTestDrawDto Dto { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value) && value)
            {
                _ = _parent.MarkSampleAsync(Dto);
            }
        }
    }
}

using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 3: Simple result entry screen + clear dialog.
/// Displays the ResultEntryDto fields, validates input, and dispatches EnterResultCommand.
/// Backend gates: non-simple tests rejected, reviewed results lock entry, balance blocks print.
/// </summary>
public sealed class SimpleResultEntryViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    private ResultWorklistItemDto? _item;
    private string _resultValue = string.Empty;
    private int? _resultFlag;
    private string _notes = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public SimpleResultEntryViewModel(ISender mediator, ResultErrorPresenter presenter, IDialogService dialogs)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;

        SaveCommand = new AsyncRelayCommand(async _ => await SaveAsync());
        ClearCommand = new AsyncRelayCommand(async _ => await ClearAsync());
    }

    public ResultWorklistItemDto? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public int PatientTestId => _item?.PatientTestId ?? 0;
    public int PatientId => _item?.PatientId ?? 0;
    public string TestName => _item?.TestName ?? string.Empty;
    public string TestCode => _item?.TestCode ?? string.Empty;
    public int ResultKind => _item?.ResultKind ?? 0;

    public string ResultValue
    {
        get => _resultValue;
        set => SetProperty(ref _resultValue, value);
    }

    public int? ResultFlag
    {
        get => _resultFlag;
        set => SetProperty(ref _resultFlag, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public ObservableCollection<ReferenceRangeDto> ReferenceRanges { get; } = new();
    public FrozenRangeDto? FrozenRange { get; private set; }

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand ClearCommand { get; }

    public async Task LoadAsync(ResultWorklistItemDto item)
    {
        // S-04 Slice 3: load reference ranges for this test
        Item = item;
        ResultValue = item.ResultValue ?? string.Empty;
        ResultFlag = item.ResultFlag;
        ErrorMessage = string.Empty;
        // Reference ranges loaded client-side if needed; backend provides via GetPatientAccountQuery pattern
        await Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ResultValue))
        {
            ErrorMessage = "قيمة النتيجة مطلوبة.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new EnterResultCommand(
                PatientTestId: PatientTestId,
                ResultValue: ResultValue,
                ResultFlag: ResultFlag,
                Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes), CancellationToken.None);

            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            // S-04 Slice 3: clear form after successful save
            await ClearAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearAsync()
    {
        var confirm = await _dialogs.ShowConfirmationAsync("مسح النتيجة", "هل أنت متأكد من مسح هذه النتيجة؟");
        if (!confirm)
        {
            return;
        }

        // Clear form but keep reference to the item for UI refresh
        ResultValue = string.Empty;
        ResultFlag = null;
        Notes = string.Empty;
    }
}
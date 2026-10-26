using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ReportProduction.Common;
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
    private readonly IResultPrintCoordinator _printCoordinator;
    private readonly IPrintedStateRecorder _printedState;
    private readonly IPdfPreviewService _pdfPreview;
    private readonly ISimpleResultPreviewBuilder _previewBuilder;

    private ResultWorklistItemDto? _item;
    private string _resultValue = string.Empty;
    private int? _resultFlag;
    private string _notes = string.Empty;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public SimpleResultEntryViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IResultPrintCoordinator printCoordinator,
        IPrintedStateRecorder printedState,
        IPdfPreviewService pdfPreview,
        ISimpleResultPreviewBuilder previewBuilder)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _printCoordinator = printCoordinator;
        _printedState = printedState;
        _pdfPreview = pdfPreview;
        _previewBuilder = previewBuilder;

        SaveCommand = new AsyncRelayCommand(async _ => await SaveAsync());
        ClearCommand = new AsyncRelayCommand(async _ => await ClearAsync());
        OpenCommentPickerCommand = new AsyncRelayCommand(async _ => await OpenCommentPickerAsync());
        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
        PreviewCommand = new AsyncRelayCommand(async (_, ct) => await PreviewAsync(ct));
        ReviewCommand = new AsyncRelayCommand(async (_, ct) => await ReviewAsync(ct));
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
    public AsyncRelayCommand OpenCommentPickerCommand { get; }

    /// <summary>1.22.12.2 «طباعة»: prints the report of the test through the shared coordinator.</summary>
    public AsyncRelayCommand PrintCommand { get; }

    /// <summary>1.22.12.3 «معاينة الطباعة»: shows the report before it is printed.</summary>
    public AsyncRelayCommand PreviewCommand { get; }

    /// <summary>1.22.12.4 «مراجعة»: opens the result for review.</summary>
    public AsyncRelayCommand ReviewCommand { get; }

    /// <summary>True once a result row is loaded, i.e. the three actions have something to act on.</summary>
    public bool HasResult => PatientTestId > 0;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

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

    /// <summary>W-02 S8 (WP-13): standard test comments — picking never writes to the database.</summary>
    private async Task OpenCommentPickerAsync()
    {
        if (string.IsNullOrWhiteSpace(TestCode))
        {
            ErrorMessage = "اختر تحليلاً أولاً قبل اختيار تعليق.";
            return;
        }

        ErrorMessage = string.Empty;

        var vm = new TestCommentPickerViewModel(_mediator, _presenter);
        await vm.LoadByTestCodeAsync(TestCode);
        var window = new Views.Patients.TestCommentPickerWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        bool? picked = window.ShowDialog();
        if (picked == true && !string.IsNullOrWhiteSpace(vm.PickedCommentText))
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? vm.PickedCommentText!
                : Notes + Environment.NewLine + vm.PickedCommentText;
        }
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

    /// <summary>
    /// 1.22.12.2 «طباعة»: the SAME shared print path the profile and culture screens use
    /// (<see cref="IResultPrintCoordinator"/> with <see cref="ResultPrintKind.SimpleResult"/>,
    /// then <see cref="IPrintedStateRecorder"/>). No second implementation.
    /// </summary>
    private async Task PrintAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!HasResult)
        {
            ErrorMessage = "اختر نتيجة محفوظة أولًا قبل الطباعة.";
            return;
        }

        IsBusy = true;
        try
        {
            var outcome = await _printCoordinator.PrintAsync(PatientTestId, ResultPrintKind.SimpleResult, cancellationToken);
            if (!outcome.Printed)
            {
                ErrorMessage = "تعذّرت الطباعة: " + outcome.ErrorMessage;
                return;
            }

            var recorded = await _printedState.RecordForPatientTestAsync(PatientTestId, cancellationToken);
            if (!recorded.IsSuccess)
            {
                ErrorMessage = recorded.Error!.Message;
                return;
            }

            StatusMessage = "تمت الطباعة.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 1.22.12.3 «معاينة الطباعة»: builds the same simple-result report and opens it for
    /// review through <see cref="IPdfPreviewService"/> — never sends it to a printer.
    /// </summary>
    private async Task PreviewAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!HasResult)
        {
            ErrorMessage = "اختر نتيجة محفوظة أولًا قبل المعاينة.";
            return;
        }

        IsBusy = true;
        try
        {
            var built = await _previewBuilder.BuildAsync(PatientTestId, cancellationToken);
            if (!built.IsSuccess)
            {
                ErrorMessage = built.Error?.Message ?? "تعذّر بناء تقرير التحليل.";
                return;
            }

            var preview = await _pdfPreview.PreviewAsync(built.Value!, cancellationToken);
            if (!preview.IsSuccess)
            {
                ErrorMessage = preview.Error?.Message ?? "تعذّر فتح معاينة التقرير.";
                return;
            }

            StatusMessage = "تم فتح المعاينة.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 1.22.12.4 «مراجعة»: dispatches the existing <c>ReviewResultCommand</c> — the same
    /// Application handler the worklist uses. The result must be saved first.
    /// </summary>
    private async Task ReviewAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!HasResult)
        {
            ErrorMessage = "اختر نتيجة محفوظة أولًا قبل المراجعة.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ResultValue))
        {
            ErrorMessage = "لا يمكن اعتماد نتيجة غير مدخلة.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new ReviewResultCommand(PatientTestId), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            StatusMessage = "تم اعتماد النتيجة.";
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
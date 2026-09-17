using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeleteReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.MapTestToAnalyte;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UnmapTestFromAnalyte;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateReferenceRange;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTest;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetReferenceRanges;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestById;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class ResultKindOption
{
    public ResultKindOption(ResultKind value, string label)
    {
        Value = value;
        Label = label;
    }

    public ResultKind Value { get; }
    public string Label { get; }
}

public sealed class AgeUnitOption
{
    public AgeUnitOption(AgeUnit value, string label)
    {
        Value = value;
        Label = label;
    }

    public AgeUnit Value { get; }
    public string Label { get; }
}

public sealed class SexOption
{
    public SexOption(Sex? value, string label)
    {
        Value = value;
        Label = label;
    }

    public Sex? Value { get; }
    public string Label { get; }
}

/// <summary>
/// M12 test editor dialog (S-02 Slice 3): the 13 confirmed <c>TestDetailDto</c>
/// fields, the reference-range tab with the permanent no-retroactive-effect
/// note, and the single-link Test↔Analyte mapping tab.
/// </summary>
public sealed class TestEditorViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private int _editingTestId;
    private bool _isEditMode;
    private string _testCode = string.Empty;
    private string _name = string.Empty;
    private string _reportName = string.Empty;
    private string _receiptName = string.Empty;
    private int? _testGroupId;
    private string? _barcode;
    private int _completionDurationMinutes = 60;
    private bool _isSentOut;
    private decimal? _sentOutCostPrice;
    private decimal _patientPrice;
    private decimal? _labToLabPrice;
    private ResultKind _resultKind = ResultKind.Simple;
    private bool _isCultureType;

    private ObservableCollection<ReferenceRangeDto> _ranges = new();
    private ReferenceRangeDto? _selectedRange;
    private int _rangeAgeMin;
    private int _rangeAgeMax;
    private AgeUnit _rangeAgeUnit = AgeUnit.Year;
    private Sex? _rangeSex;
    private decimal _rangeMinValue;
    private decimal _rangeMaxValue;
    private string? _rangeLowComment;
    private string? _rangeHighComment;

    private ObservableCollection<AnalyteDefinitionDto> _analytes = new();
    private AnalyteDefinitionDto? _selectedAnalyte;
    private string _mappedAnalyteText = "غير مرتبط";

    private ObservableCollection<TestGroupDto> _groups = new();
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public TestEditorViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        ResultKindOptions = new List<ResultKindOption>
        {
            new(ResultKind.Simple, "بسيط"),
            new(ResultKind.SpecializedProfile, "ملف متخصص"),
            new(ResultKind.Culture, "مزرعة")
        };
        AgeUnitOptions = new List<AgeUnitOption>
        {
            new(AgeUnit.Day, "يوم"),
            new(AgeUnit.Month, "شهر"),
            new(AgeUnit.Year, "سنة")
        };
        SexOptions = new List<SexOption>
        {
            new(null, "غير محدد"),
            new(Sex.Male, "ذكر"),
            new(Sex.Female, "أنثى")
        };

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        NewRangeCommand = new RelayCommand(_ => ClearRangeEditor());
        SaveRangeCommand = new AsyncRelayCommand(_ => SaveRangeAsync());
        DeleteRangeCommand = new AsyncRelayCommand(_ => DeleteRangeAsync());
        MapAnalyteCommand = new AsyncRelayCommand(_ => MapAnalyteAsync());
        UnmapAnalyteCommand = new AsyncRelayCommand(_ => UnmapAnalyteAsync());
    }

    public IReadOnlyList<ResultKindOption> ResultKindOptions { get; }
    public IReadOnlyList<AgeUnitOption> AgeUnitOptions { get; }
    public IReadOnlyList<SexOption> SexOptions { get; }
    public ObservableCollection<TestGroupDto> Groups { get => _groups; private set => SetProperty(ref _groups, value); }

    public int EditingTestId { get => _editingTestId; private set => SetProperty(ref _editingTestId, value); }
    public bool IsEditMode { get => _isEditMode; private set => SetProperty(ref _isEditMode, value); }

    public string TestCode { get => _testCode; set => SetProperty(ref _testCode, value); }
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string ReportName { get => _reportName; set => SetProperty(ref _reportName, value); }
    public string ReceiptName { get => _receiptName; set => SetProperty(ref _receiptName, value); }
    public int? TestGroupId { get => _testGroupId; set => SetProperty(ref _testGroupId, value); }
    public string? Barcode { get => _barcode; set => SetProperty(ref _barcode, value); }
    public int CompletionDurationMinutes { get => _completionDurationMinutes; set => SetProperty(ref _completionDurationMinutes, value); }
    public bool IsSentOut { get => _isSentOut; set => SetProperty(ref _isSentOut, value); }
    public decimal? SentOutCostPrice { get => _sentOutCostPrice; set => SetProperty(ref _sentOutCostPrice, value); }
    public decimal PatientPrice { get => _patientPrice; set => SetProperty(ref _patientPrice, value); }
    public decimal? LabToLabPrice { get => _labToLabPrice; set => SetProperty(ref _labToLabPrice, value); }
    public ResultKind ResultKind { get => _resultKind; set => SetProperty(ref _resultKind, value); }
    public bool IsCultureType { get => _isCultureType; set => SetProperty(ref _isCultureType, value); }

    public ObservableCollection<ReferenceRangeDto> Ranges { get => _ranges; private set => SetProperty(ref _ranges, value); }

    public ReferenceRangeDto? SelectedRange
    {
        get => _selectedRange;
        set
        {
            if (SetProperty(ref _selectedRange, value) && value is not null)
            {
                RangeAgeMin = value.AgeMin;
                RangeAgeMax = value.AgeMax;
                RangeAgeUnit = value.AgeUnit;
                RangeSex = value.Sex;
                RangeMinValue = value.MinValue;
                RangeMaxValue = value.MaxValue;
                RangeLowComment = value.LowComment;
                RangeHighComment = value.HighComment;
            }
        }
    }

    public int RangeAgeMin { get => _rangeAgeMin; set => SetProperty(ref _rangeAgeMin, value); }
    public int RangeAgeMax { get => _rangeAgeMax; set => SetProperty(ref _rangeAgeMax, value); }
    public AgeUnit RangeAgeUnit { get => _rangeAgeUnit; set => SetProperty(ref _rangeAgeUnit, value); }
    public Sex? RangeSex { get => _rangeSex; set => SetProperty(ref _rangeSex, value); }
    public decimal RangeMinValue { get => _rangeMinValue; set => SetProperty(ref _rangeMinValue, value); }
    public decimal RangeMaxValue { get => _rangeMaxValue; set => SetProperty(ref _rangeMaxValue, value); }
    public string? RangeLowComment { get => _rangeLowComment; set => SetProperty(ref _rangeLowComment, value); }
    public string? RangeHighComment { get => _rangeHighComment; set => SetProperty(ref _rangeHighComment, value); }

    public ObservableCollection<AnalyteDefinitionDto> Analytes { get => _analytes; private set => SetProperty(ref _analytes, value); }
    public AnalyteDefinitionDto? SelectedAnalyte { get => _selectedAnalyte; set => SetProperty(ref _selectedAnalyte, value); }
    public string MappedAnalyteText { get => _mappedAnalyteText; private set => SetProperty(ref _mappedAnalyteText, value); }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand NewRangeCommand { get; }
    public AsyncRelayCommand SaveRangeCommand { get; }
    public AsyncRelayCommand DeleteRangeCommand { get; }
    public AsyncRelayCommand MapAnalyteCommand { get; }
    public AsyncRelayCommand UnmapAnalyteCommand { get; }

    public void InitializeNew()
    {
        EditingTestId = 0;
        IsEditMode = false;
        MappedAnalyteText = "غير مرتبط";
    }

    public void InitializeEdit(int testId)
    {
        EditingTestId = testId;
        IsEditMode = true;
    }

    public async Task LoadDetailAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var groupsResult = await _mediator.Send(new GetTestGroupsQuery(IncludeInactive: true));
            if (groupsResult.IsSuccess && groupsResult.Value is not null)
            {
                Groups = new ObservableCollection<TestGroupDto>(groupsResult.Value);
            }

            var analytesResult = await _mediator.Send(new GetAnalyteDefinitionsQuery());
            if (analytesResult.IsSuccess && analytesResult.Value is not null)
            {
                Analytes = new ObservableCollection<AnalyteDefinitionDto>(analytesResult.Value);
            }

            if (!IsEditMode)
            {
                return;
            }

            var detail = await _mediator.Send(new GetTestByIdQuery(EditingTestId));
            if (detail.IsSuccess && detail.Value is not null)
            {
                var d = detail.Value;
                TestCode = d.TestCode;
                Name = d.Name;
                ReportName = d.ReportName;
                ReceiptName = d.ReceiptName;
                TestGroupId = d.TestGroupId;
                Barcode = d.Barcode;
                CompletionDurationMinutes = d.CompletionDurationMinutes;
                IsSentOut = d.IsSentOut;
                SentOutCostPrice = d.SentOutCostPrice;
                PatientPrice = d.PatientPrice;
                LabToLabPrice = d.LabToLabPrice;
                ResultKind = (ResultKind)d.ResultKind;
                IsCultureType = d.IsCultureType;
            }
            else if (detail.Error is not null)
            {
                ErrorMessage = _presenter.Present(detail.Error);
                return;
            }

            await LoadRangesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (!IsEditMode)
            {
                var result = await _mediator.Send(new CreateTestCommand(
                    Name.Trim(), ReportName.Trim(), ReceiptName.Trim(), TestCode.Trim(),
                    CompletionDurationMinutes, PatientPrice, ResultKind, IsCultureType,
                    TestGroupId, string.IsNullOrWhiteSpace(Barcode) ? null : Barcode.Trim(),
                    IsSentOut, SentOutCostPrice, LabToLabPrice));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                EditingTestId = result.Value;
                IsEditMode = true;
                StatusMessage = "تم حفظ التحليل.";
                await LoadRangesAsync();
            }
            else
            {
                var result = await _mediator.Send(new UpdateTestCommand(
                    EditingTestId, Name.Trim(), ReportName.Trim(), ReceiptName.Trim(), TestCode.Trim(),
                    CompletionDurationMinutes, PatientPrice, TestGroupId,
                    string.IsNullOrWhiteSpace(Barcode) ? null : Barcode.Trim(),
                    IsSentOut, SentOutCostPrice, LabToLabPrice));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ التحليل.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadRangesAsync()
    {
        if (EditingTestId <= 0)
        {
            return;
        }

        var result = await _mediator.Send(new GetReferenceRangesQuery(EditingTestId));
        if (result.IsSuccess && result.Value is not null)
        {
            Ranges = new ObservableCollection<ReferenceRangeDto>(result.Value);
        }
        else if (result.Error is not null)
        {
            ErrorMessage = _presenter.Present(result.Error);
        }
    }

    private void ClearRangeEditor()
    {
        SelectedRange = null;
        RangeAgeMin = 0;
        RangeAgeMax = 0;
        RangeAgeUnit = AgeUnit.Year;
        RangeSex = null;
        RangeMinValue = 0;
        RangeMaxValue = 0;
        RangeLowComment = null;
        RangeHighComment = null;
    }

    public async Task SaveRangeAsync()
    {
        if (EditingTestId <= 0)
        {
            ErrorMessage = "احفظ بيانات التحليل أولاً قبل إضافة المدى المرجعي.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            if (SelectedRange is null)
            {
                var result = await _mediator.Send(new CreateReferenceRangeCommand(
                    EditingTestId, RangeSex, RangeAgeUnit, RangeAgeMin, RangeAgeMax,
                    RangeMinValue, RangeMaxValue, RangeLowComment, RangeHighComment));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }
            }
            else
            {
                var result = await _mediator.Send(new UpdateReferenceRangeCommand(
                    SelectedRange.Id, RangeSex, RangeAgeUnit, RangeAgeMin, RangeAgeMax,
                    RangeMinValue, RangeMaxValue, RangeLowComment, RangeHighComment));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }
            }

            ClearRangeEditor();
            await LoadRangesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeleteRangeAsync()
    {
        if (SelectedRange is null)
        {
            ErrorMessage = "اختر مدى مرجعياً لحذفه.";
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync("حذف المدى المرجعي", "سيتم حذف المدى المرجعي المحدد — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new DeleteReferenceRangeCommand(SelectedRange.Id));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        ClearRangeEditor();
        await LoadRangesAsync();
    }

    public async Task MapAnalyteAsync()
    {
        if (EditingTestId <= 0)
        {
            ErrorMessage = "احفظ بيانات التحليل أولاً قبل الربط.";
            return;
        }

        if (SelectedAnalyte is null)
        {
            ErrorMessage = "اختر مكوّناً لربطه.";
            return;
        }

        if (MappedAnalyteText == SelectedAnalyte.Name)
        {
            ErrorMessage = "هذا المكوّن مرتبط بالفعل";
            return;
        }

        var result = await _mediator.Send(new MapTestToAnalyteCommand(EditingTestId, SelectedAnalyte.AnalyteId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        MappedAnalyteText = SelectedAnalyte.Name;
        StatusMessage = "تم الربط.";
    }

    public async Task UnmapAnalyteAsync()
    {
        if (EditingTestId <= 0)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync("فك الربط", "سيتم فك ربط المكوّن عن التحليل — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new UnmapTestFromAnalyteCommand(EditingTestId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        MappedAnalyteText = "غير مرتبط";
        StatusMessage = "تم فك الربط.";
    }
}

using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportItemTemplates;
using TopLab.Domain.Settings;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>GAP-014 (Phase 7): one editable report-item template row.</summary>
public sealed class ReportItemTemplateRow : ViewModelBase
{
    private string _label = string.Empty;
    private string _referenceValueExpression = string.Empty;
    private bool _showHide = true;
    private string _listMembership = string.Empty;
    private bool _freeTextOverride;

    public ReportItemTemplateRow(string itemKey, string caption)
    {
        ItemKey = itemKey;
        Caption = caption;
    }

    public string ItemKey { get; }
    public string Caption { get; }

    public string Label { get => _label; set => SetProperty(ref _label, value); }
    public string ReferenceValueExpression { get => _referenceValueExpression; set => SetProperty(ref _referenceValueExpression, value); }
    public bool ShowHide { get => _showHide; set => SetProperty(ref _showHide, value); }
    public string ListMembership { get => _listMembership; set => SetProperty(ref _listMembership, value); }
    public bool FreeTextOverride { get => _freeTextOverride; set => SetProperty(ref _freeTextOverride, value); }
}

/// <summary>S-32 (GAP-014): edits the labels, reference expressions, visibility,
/// list membership and free-text flags of every report-item template.</summary>
public sealed class ReportItemTemplatesViewModel : ViewModelBase
{
    private static readonly IReadOnlyDictionary<string, string> Captions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ReportItemKeys.GroupSubtitle] = "عنوان المجموعة",
            [ReportItemKeys.TestName] = "اسم التحليل",
            [ReportItemKeys.ResultValue] = "قيمة النتيجة",
            [ReportItemKeys.OffLabNote] = "ملاحظة سحب العينة خارج المعمل",
            [ReportItemKeys.ReferenceRange] = "المدى المرجعي",
            [ReportItemKeys.LowComment] = "تعليق القيمة المنخفضة",
            [ReportItemKeys.HighComment] = "تعليق القيمة المرتفعة",
            [ReportItemKeys.TestComment] = "تعليق التحليل",
            [ReportItemKeys.ProfileAnalyte] = "محلل داخل تحليل مركّب",
            [ReportItemKeys.ProfileReferenceRange] = "المدى المرجعي للمحلل",
            [ReportItemKeys.ProfileLowComment] = "تعليق انخفاض المحلل",
            [ReportItemKeys.ProfileHighComment] = "تعليق ارتفاع المحلل",
            [ReportItemKeys.CultureLine] = "سطر المزرعة",
            [ReportItemKeys.MicroscopyLine] = "سطر الفحص المجهري",
            [ReportItemKeys.CultureSensitivityGrid] = "جدول الحساسية",
            [ReportItemKeys.HistoryDate] = "تاريخ التاريخ المرضي",
            [ReportItemKeys.HistoryTest] = "تحليل التاريخ المرضي",
            [ReportItemKeys.HistoryResult] = "نتيجة التاريخ المرضي",
            [ReportItemKeys.HistoryReviewed] = "حالة الاعتماد",
            [ReportItemKeys.HistoryComment] = "تعليق التاريخ المرضي",
            [ReportItemKeys.PatientExportTestLine] = "سطر تحليل تصدير المريض",
            [ReportItemKeys.PatientExportProfileItems] = "عناصر التحليل المركّب للتصدير",
            [ReportItemKeys.PatientExportCultureSummary] = "ملخص المزرعة للتصدير",
            [ReportItemKeys.ProfileReportComment] = "تعليق تقرير التحليل المركّب"
        };

    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private bool _hasLoaded;

    public ReportItemTemplatesViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        Rows = ReportItemKeys.All
            .Select(key => new ReportItemTemplateRow(key, Captions.TryGetValue(key, out var caption) ? caption : key))
            .ToList();

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        BackToDashboardCommand = new RelayCommand(_ => _navigation.NavigateTo<SettingsDashboardViewModel>());
    }

    public IReadOnlyList<ReportItemTemplateRow> Rows { get; }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand BackToDashboardCommand { get; }

    public async Task LoadAsync()
    {
        if (_hasLoaded)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetReportItemTemplatesQuery());
            if (result.IsSuccess && result.Value is not null)
            {
                var byKey = result.Value.ToDictionary(i => i.ItemKey, StringComparer.Ordinal);
                foreach (var row in Rows)
                {
                    if (byKey.TryGetValue(row.ItemKey, out var dto))
                    {
                        row.Label = dto.Label;
                        row.ReferenceValueExpression = dto.ReferenceValueExpression ?? string.Empty;
                        row.ShowHide = dto.ShowHide;
                        row.ListMembership = dto.ListMembership ?? string.Empty;
                        row.FreeTextOverride = dto.FreeTextOverride;
                    }
                }

                _hasLoaded = true;
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

    public async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var items = Rows
            .Select(row => new ReportItemTemplateDto(
                row.ItemKey,
                row.Label,
                string.IsNullOrWhiteSpace(row.ReferenceValueExpression) ? null : row.ReferenceValueExpression,
                row.ShowHide,
                string.IsNullOrWhiteSpace(row.ListMembership) ? null : row.ListMembership,
                row.FreeTextOverride))
            .ToList();

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new UpdateReportItemTemplatesCommand(items));
            if (result.IsSuccess)
            {
                StatusMessage = "تم حفظ عناصر التقرير.";
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

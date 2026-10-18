using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Domain.Common.Enums;
using Microsoft.Win32;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>S-29: report-print settings plus the lab identification text block and font.</summary>
public sealed class ReportSettingsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;
    private readonly IReportImageAssetStore? _imageAssetStore;

    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private bool _hasLoaded;

    private decimal _pageMarginLeftCm = 1.0m;
    private decimal _pageMarginBottomCm = 1.0m;
    private decimal _reportTopSpaceCm = 2.0m;
    private PaperSize _paperSize = PaperSize.A4;
    private HeaderFooterMode _headerFooterMode = HeaderFooterMode.None;
    private bool _doctorSignatureEnabled;
    private HistorySortMode _historySortMode = HistorySortMode.ByLabCode;
    private bool _historyAutoDisplayEnabled = true;

    private string _labName = string.Empty;
    private string _labAddress = string.Empty;
    private string _labPhone = string.Empty;
    private string _fontFamily = string.Empty;
    private int _fontSizePt = 12;
    private string? _headerColor;
    private string? _footerColor;
    private string? _headerImagePath;
    private string? _footerImagePath;
    private string? _normalRangeMarker;
    private bool _enableAbnormalResultColor;
    private string? _abnormalResultFontColor;
    private string? _abnormalResultBackgroundColor;

    public ReportSettingsViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        INavigationService navigation,
        IReportImageAssetStore? imageAssetStore = null)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;
        _imageAssetStore = imageAssetStore;

        PaperSizeOptions = new List<PaperSize> { PaperSize.A4, PaperSize.A5 };
        HeaderFooterModeOptions = new List<HeaderFooterMode> { HeaderFooterMode.None, HeaderFooterMode.Words, HeaderFooterMode.Images };
        HistorySortModeOptions = new List<HistorySortMode> { HistorySortMode.ByLabCode, HistorySortMode.ByPatientName };
        FontFamilyOptions = new List<string> { "Arial", "Tahoma", "Calibri", "Times New Roman", "Palatino Linotype" };

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        BackToDashboardCommand = new RelayCommand(_ => _navigation.NavigateTo<SettingsDashboardViewModel>());
        PickHeaderImageCommand = new RelayCommand(_ => PickHeaderImage());
        PickFooterImageCommand = new RelayCommand(_ => PickFooterImage());
        ClearHeaderImageCommand = new RelayCommand(_ => { HeaderImagePath = null; StatusMessage = "تمت إزالة صورة الرأس."; });
        ClearFooterImageCommand = new RelayCommand(_ => { FooterImagePath = null; StatusMessage = "تمت إزالة صورة التذييل."; });
    }

    public IReadOnlyList<PaperSize> PaperSizeOptions { get; }
    public IReadOnlyList<HeaderFooterMode> HeaderFooterModeOptions { get; }
    public IReadOnlyList<HistorySortMode> HistorySortModeOptions { get; }
    public IReadOnlyList<string> FontFamilyOptions { get; }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand BackToDashboardCommand { get; }
    public RelayCommand PickHeaderImageCommand { get; }
    public RelayCommand PickFooterImageCommand { get; }
    public RelayCommand ClearHeaderImageCommand { get; }
    public RelayCommand ClearFooterImageCommand { get; }

    public decimal PageMarginLeftCm { get => _pageMarginLeftCm; set => SetProperty(ref _pageMarginLeftCm, value); }
    public decimal PageMarginBottomCm { get => _pageMarginBottomCm; set => SetProperty(ref _pageMarginBottomCm, value); }
    public decimal ReportTopSpaceCm { get => _reportTopSpaceCm; set => SetProperty(ref _reportTopSpaceCm, value); }
    public PaperSize PaperSize { get => _paperSize; set => SetProperty(ref _paperSize, value); }
    public HeaderFooterMode HeaderFooterMode { get => _headerFooterMode; set => SetProperty(ref _headerFooterMode, value); }
    public bool DoctorSignatureEnabled { get => _doctorSignatureEnabled; set => SetProperty(ref _doctorSignatureEnabled, value); }
    public HistorySortMode HistorySortMode { get => _historySortMode; set => SetProperty(ref _historySortMode, value); }
    public bool HistoryAutoDisplayEnabled { get => _historyAutoDisplayEnabled; set => SetProperty(ref _historyAutoDisplayEnabled, value); }

    public string LabName { get => _labName; set => SetProperty(ref _labName, value); }
    public string LabAddress { get => _labAddress; set => SetProperty(ref _labAddress, value); }
    public string LabPhone { get => _labPhone; set => SetProperty(ref _labPhone, value); }
    public string FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }
    public int FontSizePt { get => _fontSizePt; set => SetProperty(ref _fontSizePt, value); }

    public string? HeaderColor { get => _headerColor; set => SetProperty(ref _headerColor, value); }
    public string? FooterColor { get => _footerColor; set => SetProperty(ref _footerColor, value); }
    public string? HeaderImagePath { get => _headerImagePath; set => SetProperty(ref _headerImagePath, value); }
    public string? FooterImagePath { get => _footerImagePath; set => SetProperty(ref _footerImagePath, value); }
    public string? NormalRangeMarker { get => _normalRangeMarker; set => SetProperty(ref _normalRangeMarker, value); }
    public bool EnableAbnormalResultColor { get => _enableAbnormalResultColor; set => SetProperty(ref _enableAbnormalResultColor, value); }
    public string? AbnormalResultFontColor { get => _abnormalResultFontColor; set => SetProperty(ref _abnormalResultFontColor, value); }
    public string? AbnormalResultBackgroundColor { get => _abnormalResultBackgroundColor; set => SetProperty(ref _abnormalResultBackgroundColor, value); }

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
            var settings = await _mediator.Send(new GetReportSettingsQuery());
            if (settings.IsSuccess && settings.Value is not null)
            {
                var s = settings.Value;
                PageMarginLeftCm = s.PageMarginLeftCm;
                PageMarginBottomCm = s.PageMarginBottomCm;
                ReportTopSpaceCm = s.ReportTopSpaceCm;
                PaperSize = s.PaperSize;
                HeaderFooterMode = s.HeaderFooterMode;
                DoctorSignatureEnabled = s.DoctorSignatureEnabled;
                HistorySortMode = s.HistorySortMode;
                HistoryAutoDisplayEnabled = s.HistoryAutoDisplayEnabled;
                HeaderColor = s.HeaderColor;
                FooterColor = s.FooterColor;
                HeaderImagePath = s.HeaderImagePath;
                FooterImagePath = s.FooterImagePath;
                NormalRangeMarker = s.NormalRangeMarker;
                EnableAbnormalResultColor = s.EnableAbnormalResultColor;
                AbnormalResultFontColor = s.AbnormalResultFontColor;
                AbnormalResultBackgroundColor = s.AbnormalResultBackgroundColor;
            }
            else if (settings.Error is not null)
            {
                ErrorMessage = _presenter.Present(settings.Error);
            }

            var lab = await _mediator.Send(new GetLabPrintTextQuery { Scope = LabPrintTextScope.Report });
            if (lab.IsSuccess && lab.Value is not null)
            {
                LabName = lab.Value.LabName;
                LabAddress = lab.Value.Address;
                LabPhone = lab.Value.Phone;
                FontFamily = lab.Value.FontFamily;
                if (lab.Value.FontSizePt > 0)
                {
                    FontSizePt = lab.Value.FontSizePt;
                }
            }

            _hasLoaded = true;
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

        if (ReportTopSpaceCm > 8.0m)
        {
            ErrorMessage = "الهامش العلوي للتقرير لا يمكن أن يتجاوز 8 سم";
            return;
        }

        if (string.IsNullOrWhiteSpace(LabName))
        {
            ErrorMessage = "اسم المعمل مطلوب.";
            return;
        }

        IsBusy = true;
        try
        {
            var settingsResult = await _mediator.Send(new UpdateReportSettingsCommand(
                PageMarginLeftCm,
                PageMarginBottomCm,
                ReportTopSpaceCm,
                PaperSize,
                HeaderFooterMode,
                DoctorSignatureEnabled,
                HistorySortMode,
                HistoryAutoDisplayEnabled,
                HeaderColor: HeaderColor,
                FooterColor: FooterColor,
                HeaderImagePath: HeaderImagePath,
                FooterImagePath: FooterImagePath,
                NormalRangeMarker: NormalRangeMarker,
                EnableAbnormalResultColor: EnableAbnormalResultColor,
                AbnormalResultFontColor: AbnormalResultFontColor,
                AbnormalResultBackgroundColor: AbnormalResultBackgroundColor));
            if (!settingsResult.IsSuccess)
            {
                ErrorMessage = settingsResult.Error is not null ? _presenter.Present(settingsResult.Error) : ErrorMessage;
                return;
            }

            var labResult = await _mediator.Send(new SaveLabPrintTextCommand(
                LabPrintTextScope.Report,
                LabName,
                LabAddress,
                LabPhone,
                FontFamily,
                FontSizePt));
            if (!labResult.IsSuccess)
            {
                ErrorMessage = labResult.Error is not null ? _presenter.Present(labResult.Error) : ErrorMessage;
                return;
            }

            StatusMessage = "تم حفظ إعدادات التقرير.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PickHeaderImage()
    {
        var path = PickImageFile();
        if (path is null)
        {
            return;
        }

        if (_imageAssetStore is null)
        {
            ErrorMessage = "خدمة تخزين الصور غير متوفرة.";
            return;
        }

        var storedPath = _imageAssetStore.StoreImage(path, ReportImageTarget.Header);
        if (storedPath is not null)
        {
            HeaderImagePath = storedPath;
            StatusMessage = "تم اختيار صورة الرأس.";
        }
        else
        {
            ErrorMessage = "تعذر تخزين الصورة. تأكد من أن الصيغة مدعومة (PNG, JPG, BMP, GIF) والحجم أقل من 5 ميجابايت.";
        }
    }

    private void PickFooterImage()
    {
        var path = PickImageFile();
        if (path is null)
        {
            return;
        }

        if (_imageAssetStore is null)
        {
            ErrorMessage = "خدمة تخزين الصور غير متوفرة.";
            return;
        }

        var storedPath = _imageAssetStore.StoreImage(path, ReportImageTarget.Footer);
        if (storedPath is not null)
        {
            FooterImagePath = storedPath;
            StatusMessage = "تم اختيار صورة التذييل.";
        }
        else
        {
            ErrorMessage = "تعذر تخزين الصورة. تأكد من أن الصيغة مدعومة (PNG, JPG, BMP, GIF) والحجم أقل من 5 ميجابايت.";
        }
    }

    private static string? PickImageFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "اختر صورة",
            Filter = "ملفات الصور (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|جميع الملفات (*.*)|*.*"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetEnvelopeSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>One editable envelope print item position row.</summary>
public sealed class EnvelopePositionRow : ViewModelBase
{
    private bool _isEnabled;
    private decimal _leftOffsetCm;
    private decimal _topOffsetCm;

    public EnvelopePositionRow(string itemName, string caption)
    {
        ItemName = itemName;
        Caption = caption;
    }

    public string ItemName { get; }
    public string Caption { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public decimal LeftOffsetCm
    {
        get => _leftOffsetCm;
        set => SetProperty(ref _leftOffsetCm, value);
    }

    public decimal TopOffsetCm
    {
        get => _topOffsetCm;
        set => SetProperty(ref _topOffsetCm, value);
    }
}

/// <summary>S-31: envelope-print settings, the envelope lab identification text block,
/// the four alignment rows, and the static barcode preview rectangle.</summary>
public sealed class EnvelopeSettingsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private bool _hasLoaded;

    private decimal _topMarginCm = 1.0m;
    private HeaderFooterMode _headerFooterMode = HeaderFooterMode.None;
    private bool _suppressCaptions;

    private string _labName = string.Empty;
    private string _labAddress = string.Empty;
    private string _labPhone = string.Empty;
    private string _fontFamily = "Arial";
    private int _fontSizePt = 12;

    public EnvelopeSettingsViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        EnvelopePositions = new List<EnvelopePositionRow>
        {
            new("Name", "الاسم"),
            new("Code", "الكود"),
            new("ReferralEntity", "جهة الإحالة"),
            new("Date", "التاريخ")
        };

        HeaderFooterModeOptions = new List<HeaderFooterMode> { HeaderFooterMode.None, HeaderFooterMode.Words, HeaderFooterMode.Images };
        FontFamilyOptions = new List<string> { "Arial", "Tahoma", "Calibri", "Times New Roman", "Palatino Linotype" };

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        BackToDashboardCommand = new RelayCommand(_ => _navigation.NavigateTo<SettingsDashboardViewModel>());
    }

    public IReadOnlyList<EnvelopePositionRow> EnvelopePositions { get; }
    public IReadOnlyList<HeaderFooterMode> HeaderFooterModeOptions { get; }
    public IReadOnlyList<string> FontFamilyOptions { get; }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand BackToDashboardCommand { get; }

    public decimal TopMarginCm { get => _topMarginCm; set => SetProperty(ref _topMarginCm, value); }
    public HeaderFooterMode HeaderFooterMode { get => _headerFooterMode; set => SetProperty(ref _headerFooterMode, value); }
    public bool SuppressCaptions { get => _suppressCaptions; set => SetProperty(ref _suppressCaptions, value); }

    public string LabName { get => _labName; set => SetProperty(ref _labName, value); }
    public string LabAddress { get => _labAddress; set => SetProperty(ref _labAddress, value); }
    public string LabPhone { get => _labPhone; set => SetProperty(ref _labPhone, value); }
    public string FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }
    public int FontSizePt { get => _fontSizePt; set => SetProperty(ref _fontSizePt, value); }

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
            var settings = await _mediator.Send(new GetEnvelopeSettingsQuery());
            if (settings.IsSuccess && settings.Value is not null)
            {
                var s = settings.Value;
                TopMarginCm = s.TopMarginCm;
                HeaderFooterMode = s.HeaderFooterMode;
                SuppressCaptions = s.SuppressCaptions;

                if (s.Positions is not null)
                {
                    foreach (var row in EnvelopePositions)
                    {
                        var match = s.Positions.FirstOrDefault(p => string.Equals(p.ItemName, row.ItemName, System.StringComparison.OrdinalIgnoreCase));
                        if (match is not null)
                        {
                            row.IsEnabled = match.IsEnabled;
                            row.LeftOffsetCm = match.LeftOffsetCm;
                            row.TopOffsetCm = match.TopOffsetCm;
                        }
                    }
                }
            }
            else if (settings.Error is not null)
            {
                ErrorMessage = _presenter.Present(settings.Error);
            }

            var lab = await _mediator.Send(new GetLabPrintTextQuery { Scope = LabPrintTextScope.Envelope });
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

        if (string.IsNullOrWhiteSpace(LabName))
        {
            ErrorMessage = "اسم المعمل مطلوب.";
            return;
        }

        var positions = EnvelopePositions
            .Select(row => new EnvelopePrintItemPositionDto(row.ItemName, row.IsEnabled, row.LeftOffsetCm, row.TopOffsetCm))
            .ToList();

        IsBusy = true;
        try
        {
            var settingsResult = await _mediator.Send(new UpdateEnvelopeSettingsCommand(
                TopMarginCm,
                HeaderFooterMode,
                SuppressCaptions,
                positions));
            if (!settingsResult.IsSuccess)
            {
                ErrorMessage = settingsResult.Error is not null ? _presenter.Present(settingsResult.Error) : ErrorMessage;
                return;
            }

            var labResult = await _mediator.Send(new SaveLabPrintTextCommand(
                LabPrintTextScope.Envelope,
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

            StatusMessage = "تم حفظ إعدادات الظرف.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.AnalyteProfiles.Commands.CreateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.SaveAnalyteReferenceRange;
using TopLab.Application.Features.AnalyteProfiles.Commands.UpdateAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class BandRow : ViewModelBase
{
    private Sex? _sex;
    private AgeUnit _ageUnit = AgeUnit.Year;
    private int _ageMin;
    private int _ageMax;
    private decimal _minValue;
    private decimal _maxValue;
    private string? _lowComment;
    private string? _highComment;

    public BandRow()
    {
    }

    public BandRow(AnalyteBandDto dto)
    {
        _sex = dto.Sex;
        _ageUnit = dto.AgeUnit;
        _ageMin = dto.AgeMin;
        _ageMax = dto.AgeMax;
        _minValue = dto.MinValue;
        _maxValue = dto.MaxValue;
        _lowComment = dto.LowComment;
        _highComment = dto.HighComment;
    }

    public Sex? Sex { get => _sex; set => SetProperty(ref _sex, value); }
    public AgeUnit AgeUnit { get => _ageUnit; set => SetProperty(ref _ageUnit, value); }
    public int AgeMin { get => _ageMin; set => SetProperty(ref _ageMin, value); }
    public int AgeMax { get => _ageMax; set => SetProperty(ref _ageMax, value); }
    public decimal MinValue { get => _minValue; set => SetProperty(ref _minValue, value); }
    public decimal MaxValue { get => _maxValue; set => SetProperty(ref _maxValue, value); }
    public string? LowComment { get => _lowComment; set => SetProperty(ref _lowComment, value); }
    public string? HighComment { get => _highComment; set => SetProperty(ref _highComment, value); }

    public AnalyteBandInput ToInput() => new(Sex, AgeUnit, AgeMin, AgeMax, MinValue, MaxValue, LowComment, HighComment);
}

/// <summary>
/// Analyte editor dialog VM (S-02 Slice 4): Name/ReportName only (no unit
/// exists in code) plus the bulk-save bands editor.
/// </summary>
public sealed class AnalyteEditorViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private int _editingAnalyteId;
    private bool _isEditMode;
    private string _name = string.Empty;
    private string _reportName = string.Empty;
    private ObservableCollection<BandRow> _bands = new();
    private BandRow? _selectedBand;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public AnalyteEditorViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        AddBandCommand = new RelayCommand(_ => AddBand());
        RemoveBandCommand = new RelayCommand(_ => RemoveSelectedBand());
        SaveBandsCommand = new AsyncRelayCommand(_ => SaveBandsAsync());
    }

    public IReadOnlyList<AgeUnitOption> AgeUnitOptions { get; } = new List<AgeUnitOption>
    {
        new(AgeUnit.Day, "يوم"),
        new(AgeUnit.Month, "شهر"),
        new(AgeUnit.Year, "سنة")
    };

    public IReadOnlyList<SexOption> SexOptions { get; } = new List<SexOption>
    {
        new(null, "غير محدد"),
        new(Sex.Male, "ذكر"),
        new(Sex.Female, "أنثى")
    };

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string ReportName { get => _reportName; set => SetProperty(ref _reportName, value); }
    public ObservableCollection<BandRow> Bands { get => _bands; private set => SetProperty(ref _bands, value); }
    public BandRow? SelectedBand { get => _selectedBand; set => SetProperty(ref _selectedBand, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand AddBandCommand { get; }
    public RelayCommand RemoveBandCommand { get; }
    public AsyncRelayCommand SaveBandsCommand { get; }

    public void InitializeNew()
    {
        _editingAnalyteId = 0;
        _isEditMode = false;
        Name = string.Empty;
        ReportName = string.Empty;
        Bands = new ObservableCollection<BandRow>();
    }

    public void InitializeEdit(AnalyteDefinitionDto dto)
    {
        _editingAnalyteId = dto.AnalyteId;
        _isEditMode = true;
        Name = dto.Name;
        ReportName = dto.ReportName;
        Bands = new ObservableCollection<BandRow>(dto.Bands.Select(b => new BandRow(b)));
    }

    public async Task SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (!_isEditMode)
            {
                var result = await _mediator.Send(new CreateAnalyteCommand(Name.Trim(), ReportName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                _editingAnalyteId = result.Value;
                _isEditMode = true;
                StatusMessage = "تم حفظ المكوّن.";
            }
            else
            {
                var result = await _mediator.Send(new UpdateAnalyteCommand(_editingAnalyteId, Name.Trim(), ReportName.Trim()));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return;
                }

                StatusMessage = "تم حفظ المكوّن.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AddBand()
    {
        var row = new BandRow();
        if (Bands.Count > 0)
        {
            var last = Bands[^1];
            row.AgeMin = last.AgeMax;
            row.AgeUnit = last.AgeUnit;
            row.Sex = last.Sex;
        }

        Bands.Add(row);
        SelectedBand = row;
    }

    private void RemoveSelectedBand()
    {
        if (SelectedBand is not null)
        {
            Bands.Remove(SelectedBand);
            SelectedBand = null;
        }
    }

    public async Task SaveBandsAsync()
    {
        if (!_isEditMode || _editingAnalyteId <= 0)
        {
            ErrorMessage = "احفظ بيانات المكوّن أولاً قبل حفظ النطاقات.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new SaveAnalyteReferenceRangeCommand(
                _editingAnalyteId, Bands.Select(b => b.ToInput()).ToList()));
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                if (result.Errors.Count > 1)
                {
                    ErrorMessage = string.Join(Environment.NewLine, _presenter.PresentAll(result.Errors));
                }

                return;
            }

            StatusMessage = "تم حفظ النطاقات.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

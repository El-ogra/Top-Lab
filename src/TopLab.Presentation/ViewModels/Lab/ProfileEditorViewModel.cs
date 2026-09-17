using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.AnalyteProfiles.Commands.AddProfileAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;
using TopLab.Application.Features.AnalyteProfiles.Commands.RemoveProfileAnalyte;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Lab;

public sealed class AnalyteCheckItem : ViewModelBase
{
    private bool _isChecked;

    public AnalyteCheckItem(AnalyteDefinitionDto analyte, bool isChecked)
    {
        Analyte = analyte;
        _isChecked = isChecked;
    }

    public AnalyteDefinitionDto Analyte { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}

/// <summary>
/// Profile composition editor (S-02 Slice 4): Name + SpecializedTestId picker
/// + FixedPrice (both confirmed on the Profile entity and always shown) with
/// components add/remove. No delete affordance (no DeleteProfile command).
/// </summary>
public sealed class ProfileEditorViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private int _editingProfileId;
    private bool _isManageMode;
    private string _name = string.Empty;
    private int? _specializedTestId;
    private decimal _fixedPrice;
    private string _specializedTestName = string.Empty;

    private ObservableCollection<TestSummaryDto> _catalogTests = new();
    private ObservableCollection<AnalyteCheckItem> _components = new();
    private ObservableCollection<AnalyteDefinitionDto> _allAnalytes = new();
    private AnalyteDefinitionDto? _pickedAnalyte;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public ProfileEditorViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        AddComponentCommand = new AsyncRelayCommand(_ => AddComponentAsync());
        RemoveComponentCommand = new AsyncRelayCommand((p, _) => RemoveComponentAsync(p as AnalyteCheckItem));
    }

    public bool IsManageMode { get => _isManageMode; private set => SetProperty(ref _isManageMode, value); }
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public int? SpecializedTestId { get => _specializedTestId; set => SetProperty(ref _specializedTestId, value); }
    public decimal FixedPrice { get => _fixedPrice; set => SetProperty(ref _fixedPrice, value); }
    public string SpecializedTestName { get => _specializedTestName; private set => SetProperty(ref _specializedTestName, value); }

    public ObservableCollection<TestSummaryDto> CatalogTests { get => _catalogTests; private set => SetProperty(ref _catalogTests, value); }
    public ObservableCollection<AnalyteCheckItem> Components { get => _components; private set => SetProperty(ref _components, value); }
    public ObservableCollection<AnalyteDefinitionDto> AllAnalytes { get => _allAnalytes; private set => SetProperty(ref _allAnalytes, value); }
    public AnalyteDefinitionDto? PickedAnalyte { get => _pickedAnalyte; set => SetProperty(ref _pickedAnalyte, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand AddComponentCommand { get; }
    public AsyncRelayCommand RemoveComponentCommand { get; }

    public async Task InitializeNewAsync()
    {
        IsManageMode = false;
        _editingProfileId = 0;
        await ReloadLookupsAsync(Array.Empty<int>());
    }

    public async Task InitializeManageAsync(ProfileDefinitionDto profile)
    {
        IsManageMode = true;
        _editingProfileId = profile.ProfileId;
        Name = profile.Name;
        SpecializedTestId = profile.SpecializedTestId;
        SpecializedTestName = profile.SpecializedTestName;
        FixedPrice = profile.FixedPrice;
        await ReloadLookupsAsync(profile.AnalyteIds);
    }

    private async Task ReloadLookupsAsync(IReadOnlyList<int> memberIds)
    {
        IsBusy = true;
        try
        {
            var catalogResult = await _mediator.Send(new SearchTestCatalogQuery(null, null, IncludeInactive: true));
            if (catalogResult.IsSuccess && catalogResult.Value is not null)
            {
                CatalogTests = new ObservableCollection<TestSummaryDto>(catalogResult.Value);
            }

            var analytesResult = await _mediator.Send(new GetAnalyteDefinitionsQuery());
            if (analytesResult.IsSuccess && analytesResult.Value is not null)
            {
                var members = new HashSet<int>(memberIds);
                AllAnalytes = new ObservableCollection<AnalyteDefinitionDto>(analytesResult.Value);
                Components = new ObservableCollection<AnalyteCheckItem>(
                    analytesResult.Value.Select(a => new AnalyteCheckItem(a, members.Contains(a.AnalyteId))));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private IReadOnlyList<int> CheckedAnalyteIds() =>
        Components.Where(c => c.IsChecked).Select(c => c.Analyte.AnalyteId).ToList();

    public async Task SaveAsync()
    {
        if (IsManageMode)
        {
            ErrorMessage = "تعديل بيانات البروفايل غير مدعوم — تتم إدارة المكوّنات فقط.";
            return;
        }

        if (!SpecializedTestId.HasValue)
        {
            ErrorMessage = "اختر التحليل المتخصص.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new CreateProfileCommand(
                Name.Trim(), SpecializedTestId.Value, FixedPrice, CheckedAnalyteIds()));
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                return;
            }

            StatusMessage = "تم إنشاء البروفايل.";
            _editingProfileId = result.Value;
            IsManageMode = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task AddComponentAsync()
    {
        if (!IsManageMode || _editingProfileId <= 0)
        {
            ErrorMessage = "احفظ البروفايل أولاً قبل إضافة المكوّنات.";
            return;
        }

        var picked = PickedAnalyte;
        if (picked is null)
        {
            ErrorMessage = "اختر مكوّناً لإضافته.";
            return;
        }

        var current = await CurrentMemberIdsAsync();
        if (current.Contains(picked.AnalyteId))
        {
            ErrorMessage = "المكوّن مضاف بالفعل";
            return;
        }

        var result = await _mediator.Send(new AddProfileAnalyteCommand(_editingProfileId, picked.AnalyteId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        var check = Components.FirstOrDefault(c => c.Analyte.AnalyteId == picked.AnalyteId);
        if (check is not null)
        {
            check.IsChecked = true;
        }

        StatusMessage = "تمت إضافة المكوّن.";
    }

    public async Task RemoveComponentAsync(AnalyteCheckItem? item)
    {
        if (!IsManageMode || _editingProfileId <= 0 || item is null)
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "إزالة المكوّن",
            $"سيتم إزالة المكوّن \"{item.Analyte.Name}\" من البروفايل — متابعة؟");
        if (!confirm)
        {
            return;
        }

        var result = await _mediator.Send(new RemoveProfileAnalyteCommand(_editingProfileId, item.Analyte.AnalyteId));
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        item.IsChecked = false;
        StatusMessage = "تمت إزالة المكوّن.";
    }

    private async Task<HashSet<int>> CurrentMemberIdsAsync()
    {
        var profiles = await _mediator.Send(new GetProfileDefinitionsQuery());
        var match = profiles.Value?.FirstOrDefault(p => p.ProfileId == _editingProfileId);
        return match is null ? new HashSet<int>() : new HashSet<int>(match.AnalyteIds);
    }
}

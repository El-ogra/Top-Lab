using MediatR;
using TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;
using TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;
using TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.External;

/// <summary>
/// M14 external-entity editor dialog (S-02 Slice 6): type chosen at create
/// and read-only on edit; code always read-only, auto-filled post-create
/// (the generate command requires an existing entity — U-15).
/// Pricing linkage passes through untouched (out of the quoted scope).
/// </summary>
public sealed class ExternalEntityEditorViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private int _editingId;
    private bool _isEditMode;
    private EntityType _entityType = EntityType.TreatingDoctor;
    private string _code = "—";
    private string _name = string.Empty;
    private string? _city;
    private string? _address;
    private string? _phone;
    private string? _fax;
    private string? _responsiblePersonName;
    private string? _responsiblePersonPhone;
    private int? _priceListId;
    private decimal? _discountOrCommissionPercent;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public ExternalEntityEditorViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        TypeOptions = new List<EntityTypeOption>
        {
            new(EntityType.TreatingDoctor, EntityTypeOption.LabelFor(EntityType.TreatingDoctor)),
            new(EntityType.ReferralOrContract, EntityTypeOption.LabelFor(EntityType.ReferralOrContract)),
            new(EntityType.PartnerLab, EntityTypeOption.LabelFor(EntityType.PartnerLab))
        };

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
    }

    public IReadOnlyList<EntityTypeOption> TypeOptions { get; }

    public bool IsEditMode { get => _isEditMode; private set => SetProperty(ref _isEditMode, value); }
    public bool IsTypeEditable => !IsEditMode;

    public EntityType EntityType
    {
        get => _entityType;
        set
        {
            if (IsEditMode)
            {
                return;
            }

            SetProperty(ref _entityType, value);
        }
    }

    public string Code { get => _code; private set => SetProperty(ref _code, value); }
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string? City { get => _city; set => SetProperty(ref _city, value); }
    public string? Address { get => _address; set => SetProperty(ref _address, value); }
    public string? Phone { get => _phone; set => SetProperty(ref _phone, value); }
    public string? Fax { get => _fax; set => SetProperty(ref _fax, value); }
    public string? ResponsiblePersonName { get => _responsiblePersonName; set => SetProperty(ref _responsiblePersonName, value); }
    public string? ResponsiblePersonPhone { get => _responsiblePersonPhone; set => SetProperty(ref _responsiblePersonPhone, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand SaveCommand { get; }

    public void InitializeNew(EntityType? presetType)
    {
        _editingId = 0;
        IsEditMode = false;
        OnPropertyChanged(nameof(IsTypeEditable));
        _entityType = presetType ?? EntityType.TreatingDoctor;
        OnPropertyChanged(nameof(EntityType));
        Code = "—";
        Name = string.Empty;
        City = Address = Phone = Fax = ResponsiblePersonName = ResponsiblePersonPhone = null;
        _priceListId = null;
        _discountOrCommissionPercent = null;
    }

    public void InitializeEdit(int id)
    {
        _editingId = id;
        IsEditMode = true;
        OnPropertyChanged(nameof(IsTypeEditable));
    }

    public async Task LoadDetailAsync()
    {
        if (!IsEditMode)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetExternalEntityByIdQuery(_editingId));
            if (result.IsSuccess && result.Value is not null)
            {
                var d = result.Value;
                _entityType = d.EntityType;
                OnPropertyChanged(nameof(EntityType));
                Code = string.IsNullOrWhiteSpace(d.GeneratedIdCode) ? "—" : d.GeneratedIdCode;
                Name = d.Name;
                City = d.City;
                Address = d.Address;
                Phone = d.Phone;
                Fax = d.Fax;
                ResponsiblePersonName = d.ResponsiblePersonName;
                ResponsiblePersonPhone = d.ResponsiblePersonPhone;
                _priceListId = d.PriceListId;
                _discountOrCommissionPercent = d.DiscountOrCommissionPercent;
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

    public async Task<bool> SaveAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            if (!IsEditMode)
            {
                var result = await _mediator.Send(new CreateExternalEntityCommand(
                    EntityType, Name.Trim(), BlankToNull(City), BlankToNull(Address),
                    BlankToNull(Phone), BlankToNull(Fax),
                    BlankToNull(ResponsiblePersonName), BlankToNull(ResponsiblePersonPhone),
                    null, null));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return false;
                }

                _editingId = result.Value;

                var codeResult = await _mediator.Send(new GenerateEntityIdCodeCommand(_editingId));
                Code = codeResult.IsSuccess && !string.IsNullOrWhiteSpace(codeResult.Value)
                    ? codeResult.Value
                    : "—";
                if (!codeResult.IsSuccess && codeResult.Error is not null)
                {
                    ErrorMessage = _presenter.Present(codeResult.Error);
                }

                IsEditMode = true;
                OnPropertyChanged(nameof(IsTypeEditable));
                StatusMessage = "تم حفظ الجهة.";
                return true;
            }
            else
            {
                var result = await _mediator.Send(new UpdateExternalEntityCommand(
                    _editingId, EntityType, Name.Trim(), BlankToNull(City), BlankToNull(Address),
                    BlankToNull(Phone), BlankToNull(Fax),
                    BlankToNull(ResponsiblePersonName), BlankToNull(ResponsiblePersonPhone),
                    _priceListId, _discountOrCommissionPercent));
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
                    return false;
                }

                StatusMessage = "تم حفظ الجهة.";
                return true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string? BlankToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

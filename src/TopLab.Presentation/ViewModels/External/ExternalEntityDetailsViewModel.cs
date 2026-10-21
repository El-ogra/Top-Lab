using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Application.Features.ExternalEntities.Queries.GetExternalEntityById;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.External;

/// <summary>
/// STAGE-01 (baseline 1.2.2.2.10.1 / 1.2.2.2.11.1): the additional information
/// of the referring entity or the treating doctor, shown read-only. The data
/// already exists on the entity; nothing is edited here.
/// </summary>
public sealed class ExternalEntityDetailsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private string _name = string.Empty;
    private string _type = string.Empty;
    private string _code = "—";
    private string? _city;
    private string? _address;
    private string? _phone;
    private string? _fax;
    private string? _responsiblePersonName;
    private string? _responsiblePersonPhone;
    private string? _email;
    private string? _priceListName;
    private decimal? _discountOrCommissionPercent;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public ExternalEntityDetailsViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;
    }

    public string Name { get => _name; private set => SetProperty(ref _name, value); }
    public string Type { get => _type; private set => SetProperty(ref _type, value); }
    public string Code { get => _code; private set => SetProperty(ref _code, value); }
    public string? City { get => _city; private set => SetProperty(ref _city, value); }
    public string? Address { get => _address; private set => SetProperty(ref _address, value); }
    public string? Phone { get => _phone; private set => SetProperty(ref _phone, value); }
    public string? Fax { get => _fax; private set => SetProperty(ref _fax, value); }
    public string? ResponsiblePersonName { get => _responsiblePersonName; private set => SetProperty(ref _responsiblePersonName, value); }
    public string? ResponsiblePersonPhone { get => _responsiblePersonPhone; private set => SetProperty(ref _responsiblePersonPhone, value); }
    public string? Email { get => _email; private set => SetProperty(ref _email, value); }
    public string? PriceListName { get => _priceListName; private set => SetProperty(ref _priceListName, value); }
    public decimal? DiscountOrCommissionPercent { get => _discountOrCommissionPercent; private set => SetProperty(ref _discountOrCommissionPercent, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public async Task LoadAsync(int entityId)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new GetExternalEntityByIdQuery(entityId));
            if (result.IsSuccess && result.Value is not null)
            {
                Apply(result.Value);
            }
            else
            {
                ErrorMessage = result.Error is not null
                    ? _presenter.Present(result.Error)
                    : "تعذر تحميل معلومات الجهة.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Apply(ExternalEntityDetailDto dto)
    {
        Name = dto.Name;
        Type = EntityTypeOption.LabelFor(dto.EntityType);
        Code = string.IsNullOrWhiteSpace(dto.GeneratedIdCode) ? "—" : dto.GeneratedIdCode;
        City = dto.City;
        Address = dto.Address;
        Phone = dto.Phone;
        Fax = dto.Fax;
        ResponsiblePersonName = dto.ResponsiblePersonName;
        ResponsiblePersonPhone = dto.ResponsiblePersonPhone;
        Email = dto.Email;
        PriceListName = dto.PriceListName;
        DiscountOrCommissionPercent = dto.DiscountOrCommissionPercent;
    }
}

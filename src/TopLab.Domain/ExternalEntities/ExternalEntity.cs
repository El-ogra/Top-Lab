using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.ExternalEntities;

public sealed class ExternalEntity : AuditableEntity<ExternalEntityId>
{
    public EntityType EntityType { get; private set; }

    public string Name { get; private set; } = default!;

    public string? City { get; private set; }

    public string? Address { get; private set; }

    public string? Phone { get; private set; }

    public string? Fax { get; private set; }

    public string? ResponsiblePersonName { get; private set; }

    public string? ResponsiblePersonPhone { get; private set; }

    public PriceListId? PriceListId { get; private set; }

    public decimal? DiscountOrCommissionPercent { get; private set; }

    public string? GeneratedIdCode { get; private set; }

    private ExternalEntity()
    {
    }

    private ExternalEntity(
        ExternalEntityId id,
        EntityType entityType,
        string name,
        string? city,
        string? address,
        string? phone,
        string? fax,
        string? responsiblePersonName,
        string? responsiblePersonPhone,
        PriceListId? priceListId,
        decimal? discountOrCommissionPercent,
        string? generatedIdCode)
        : base(id)
    {
        EntityType = entityType;
        Name = name;
        City = city;
        Address = address;
        Phone = phone;
        Fax = fax;
        ResponsiblePersonName = responsiblePersonName;
        ResponsiblePersonPhone = responsiblePersonPhone;
        PriceListId = priceListId;
        DiscountOrCommissionPercent = discountOrCommissionPercent;
        GeneratedIdCode = generatedIdCode;
    }

    public static ExternalEntity Create(
        ExternalEntityId id,
        EntityType entityType,
        string name,
        string? city = null,
        string? address = null,
        string? phone = null,
        string? fax = null,
        string? responsiblePersonName = null,
        string? responsiblePersonPhone = null,
        PriceListId? priceListId = null,
        decimal? discountOrCommissionPercent = null,
        string? generatedIdCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (entityType == EntityType.TreatingDoctor && priceListId is not null)
        {
            throw new ArgumentException("TreatingDoctor must not have PriceListId.", nameof(priceListId));
        }

        return new ExternalEntity(id, entityType, name.Trim(), city, address, phone, fax, responsiblePersonName, responsiblePersonPhone, priceListId, discountOrCommissionPercent, generatedIdCode);
    }
}

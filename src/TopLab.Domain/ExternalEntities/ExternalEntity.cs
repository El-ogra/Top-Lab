using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.ExternalEntities;

public sealed class ExternalEntity : AuditableEntity<ExternalEntityId>
{
    public const int MaxNameLength = 200;

    public const int MaxGeneratedIdCodeLength = 50;
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
        var normalizedName = RequireName(name);
        ValidatePriceListRule(entityType, priceListId);
        ValidatePercent(discountOrCommissionPercent);

        return new ExternalEntity(
            id,
            entityType,
            normalizedName,
            Normalize(city),
            Normalize(address),
            Normalize(phone),
            Normalize(fax),
            Normalize(responsiblePersonName),
            Normalize(responsiblePersonPhone),
            priceListId,
            discountOrCommissionPercent,
            Normalize(generatedIdCode));
    }

    public void Update(
        EntityType entityType,
        string name,
        string? city = null,
        string? address = null,
        string? phone = null,
        string? fax = null,
        string? responsiblePersonName = null,
        string? responsiblePersonPhone = null,
        PriceListId? priceListId = null,
        decimal? discountOrCommissionPercent = null)
    {
        var normalizedName = RequireName(name);
        ValidatePriceListRule(entityType, priceListId);
        ValidatePercent(discountOrCommissionPercent);

        EntityType = entityType;
        Name = normalizedName;
        City = Normalize(city);
        Address = Normalize(address);
        Phone = Normalize(phone);
        Fax = Normalize(fax);
        ResponsiblePersonName = Normalize(responsiblePersonName);
        ResponsiblePersonPhone = Normalize(responsiblePersonPhone);
        PriceListId = priceListId;
        DiscountOrCommissionPercent = discountOrCommissionPercent;
    }

    public void RegenerateIdCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("GeneratedIdCode is required.", nameof(code));
        }

        var trimmed = code.Trim();
        if (trimmed.Length > MaxGeneratedIdCodeLength)
        {
            throw new ArgumentException("GeneratedIdCode must be at most 50 characters.", nameof(code));
        }

        GeneratedIdCode = trimmed;
    }

    private static string RequireName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException("Name must be at most 200 characters.", nameof(name));
        }

        return trimmed;
    }

    private static void ValidatePriceListRule(EntityType entityType, PriceListId? priceListId)
    {
        if (entityType == EntityType.TreatingDoctor && priceListId is not null)
        {
            throw new ArgumentException("TreatingDoctor must not have PriceListId.", nameof(priceListId));
        }

        if (entityType == EntityType.ReferralOrContract && priceListId is null)
        {
            throw new ArgumentException("ReferralOrContract requires PriceListId.", nameof(priceListId));
        }
    }

    private static void ValidatePercent(decimal? discountOrCommissionPercent)
    {
        if (discountOrCommissionPercent is < 0 or > 100)
        {
            throw new ArgumentException("DiscountOrCommissionPercent must be between 0 and 100.", nameof(discountOrCommissionPercent));
        }
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

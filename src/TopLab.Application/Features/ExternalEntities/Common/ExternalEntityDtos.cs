using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.ExternalEntities.Common;

public sealed record ExternalEntityListItemDto(
    int Id,
    EntityType EntityType,
    string Name,
    string? City,
    string? Phone,
    int? PriceListId,
    string? PriceListName,
    decimal? DiscountOrCommissionPercent,
    string? GeneratedIdCode);

public sealed record ExternalEntityDetailDto(
    int Id,
    EntityType EntityType,
    string Name,
    string? City,
    string? Address,
    string? Phone,
    string? Fax,
    string? ResponsiblePersonName,
    string? ResponsiblePersonPhone,
    int? PriceListId,
    string? PriceListName,
    decimal? DiscountOrCommissionPercent,
    string? GeneratedIdCode);

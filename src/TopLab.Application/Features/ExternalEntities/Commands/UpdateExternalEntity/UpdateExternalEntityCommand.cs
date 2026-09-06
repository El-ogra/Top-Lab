using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.ExternalEntities.Commands.UpdateExternalEntity;

public sealed record UpdateExternalEntityCommand(
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
    decimal? DiscountOrCommissionPercent)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}

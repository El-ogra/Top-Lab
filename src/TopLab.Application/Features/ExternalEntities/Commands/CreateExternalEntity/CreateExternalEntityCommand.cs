using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.ExternalEntities.Commands.CreateExternalEntity;

public sealed record CreateExternalEntityCommand(
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
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}

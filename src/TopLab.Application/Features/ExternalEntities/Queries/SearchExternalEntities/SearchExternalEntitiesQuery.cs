using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;

public sealed record SearchExternalEntitiesQuery(
    EntityType? EntityType,
    string? SearchTerm,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<ExternalEntityListItemDto>>>;

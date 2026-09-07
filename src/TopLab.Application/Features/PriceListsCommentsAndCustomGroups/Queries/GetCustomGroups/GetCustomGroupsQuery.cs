using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;

public sealed record GetCustomGroupsQuery() : IRequest<Result<IReadOnlyList<CustomGroupSummaryDto>>>;

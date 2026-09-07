using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupById;

public sealed record GetCustomGroupByIdQuery(int Id) : IRequest<Result<CustomGroupDetailDto>>;

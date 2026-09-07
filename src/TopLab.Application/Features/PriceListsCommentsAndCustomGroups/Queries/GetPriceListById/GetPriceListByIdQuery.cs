using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;

public sealed record GetPriceListByIdQuery(int Id) : IRequest<Result<PriceListDetailDto>>;

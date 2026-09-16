using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.GetPurchasesList;

public sealed record GetPurchasesListQuery : IRequest<Result<IReadOnlyList<PurchaseItemDto>>>;

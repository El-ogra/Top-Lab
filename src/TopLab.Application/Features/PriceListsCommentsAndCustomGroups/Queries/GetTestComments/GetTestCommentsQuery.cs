using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetTestComments;

public sealed record GetTestCommentsQuery(int? TestId = null) : IRequest<Result<IReadOnlyList<TestCommentDto>>>;

using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;

public sealed record GetResultWorklistQuery(
    DateOnly? Day = null,
    bool? HasResult = null,
    bool? IsReviewed = null,
    int? TestGroupId = null,
    int? ResultKind = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<ResultWorklistItemDto>>>;

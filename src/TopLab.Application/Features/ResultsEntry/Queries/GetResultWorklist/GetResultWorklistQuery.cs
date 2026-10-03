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
    int PageSize = 50,
    // P-01 F6: nullable tri-state. null = no filter; false = ONLY unprinted rows.
    // A bool? is required — a plain bool could not express "no filter" and would make
    // false indistinguishable from absent, which is the whole failure mode of F6.
    bool? IsPrinted = null) : IRequest<Result<IReadOnlyList<ResultWorklistItemDto>>>;

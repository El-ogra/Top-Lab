using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetReferenceRanges;

public sealed record GetReferenceRangesQuery(int? TestId = null)
    : IRequest<Result<IReadOnlyList<ReferenceRangeDto>>>;
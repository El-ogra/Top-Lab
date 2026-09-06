using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;

public sealed record SearchTestCatalogQuery(string? SearchTerm, int? TestGroupId, bool IncludeInactive = false)
    : IRequest<Result<IReadOnlyList<TestSummaryDto>>>;
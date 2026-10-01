using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestCommentsForResults;

/// <summary>W-02 S8 (WP-13): one aggregated read of the comments attached to a set
/// of tests, so report building never issues one query per test (no N+1).</summary>
public sealed record TestCommentsForTestDto(int TestId, IReadOnlyList<string> Comments);

public sealed record GetTestCommentsForResultsQuery(IReadOnlyList<int> TestIds)
    : IRequest<Result<IReadOnlyList<TestCommentsForTestDto>>>;

using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;

public sealed record GetTestGroupsQuery(bool IncludeInactive = false)
    : IRequest<Result<IReadOnlyList<TestGroupDto>>>;
using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;

/// <summary>
/// The union of the test ids that belong to at least one custom group (R1 Round 5, D-02 mode CG).
/// <para>
/// Read-only, no schema change: membership already exists as <c>CustomGroupItem</c>
/// (<c>CustomGroupId</c> + <c>TestId</c>), and the registration catalogue does not carry it.
/// The result is distinct, so a test that belongs to several groups appears once — which is what
/// the catalogue list needs, since it shows one row per test.
/// </para>
/// </summary>
public sealed record GetCustomGroupTestIdsQuery() : IRequest<Result<IReadOnlyList<int>>>;

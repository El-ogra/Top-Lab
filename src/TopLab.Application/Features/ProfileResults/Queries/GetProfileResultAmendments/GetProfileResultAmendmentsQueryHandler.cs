using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ProfileResults.Queries.GetProfileResultAmendments;

/// <summary>
/// Restricted audit read (Decision 3): history is readable only by PT_AUDIT_ACCESS
/// holders (or absolute users via the pipeline); ordinary users get the standard
/// denial message. Rows are immutable and ordered by time then id.
/// </summary>
public sealed class GetProfileResultAmendmentsQueryHandler
    : IRequestHandler<GetProfileResultAmendmentsQuery, Result<IReadOnlyList<ProfileAmendmentDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetProfileResultAmendmentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<ProfileAmendmentDto>>> Handle(
        GetProfileResultAmendmentsQuery request,
        CancellationToken cancellationToken)
    {
        var amendments = _db.Set<ProfileResultAmendment>()
            .Where(a => a.ProfileResultItemId.Value == request.ProfileResultItemId)
            .OrderBy(a => a.AmendedAtUtc)
            .ThenBy(a => a.Id.Value)
            .Select(a => new ProfileAmendmentDto(
                a.Id.Value,
                a.AmendedByUserId,
                a.AmendedAtUtc,
                a.OldResultValue,
                a.OldUnit,
                a.OldFlag == null ? null : (int)a.OldFlag.Value,
                a.NewResultValue,
                a.NewUnit,
                a.NewFlag == null ? null : (int)a.NewFlag.Value,
                a.Reason))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<ProfileAmendmentDto>>.Success(amendments));
    }
}
using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.ApplyVisitDeltas;

/// <summary>W-02 S14 (WP-29): one atomic visit edit — removals, additions and sample-flag
/// updates share a single transaction and a single save. Supersedes the three
/// separate saves the editor used to issue.</summary>
public sealed record VisitFlagUpdate(
    int PatientTestId,
    bool IsUrine,
    bool IsStool,
    bool IsBlood,
    bool IsSemen,
    bool IsCsf,
    bool IsTakenOutsideLab);

public sealed record ApplyVisitDeltasCommand(
    int PatientId,
    IReadOnlyList<int> RemovedPatientTestIds,
    IReadOnlyList<AddTestInput> AddedTests,
    IReadOnlyList<VisitFlagUpdate> FlagUpdates)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}

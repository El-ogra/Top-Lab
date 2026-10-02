using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;

/// <summary>W-02 S14 (WP-29): one atomic condition-set edit (see ApplyVisitDeltas).</summary>
public sealed record ApplyConditionDeltasCommand(int PatientId, IReadOnlyList<int> WantedMedicalConditionTypeIds)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}

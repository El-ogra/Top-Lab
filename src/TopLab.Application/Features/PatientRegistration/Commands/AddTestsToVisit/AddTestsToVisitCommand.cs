using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;

public sealed record AddTestInput(
    int TestId,
    bool IsUrine,
    bool IsStool,
    bool IsBlood,
    bool IsSemen,
    bool IsCsf,
    bool IsTakenOutsideLab);

public sealed record AddTestsToVisitCommand(
    int PatientId,
    IReadOnlyList<AddTestInput> Tests)
    : IRequest<Result<IReadOnlyList<int>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}
using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Common;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;

public sealed record GetPatientTestAuditQuery(int PatientTestId)
    : IRequest<Result<PatientTestAuditDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => AuditAccessPolicy.PtAuditAccess;
}

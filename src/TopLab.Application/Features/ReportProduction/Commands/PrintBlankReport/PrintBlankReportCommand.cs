using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;

public sealed record PrintBlankReportCommand(int PatientId) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ReportProductionAccessPolicy.PrintResults;
}
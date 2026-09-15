using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;

public sealed record PrintHistoryReportCommand(int PatientId) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ReportProductionAccessPolicy.PrintResults;
}
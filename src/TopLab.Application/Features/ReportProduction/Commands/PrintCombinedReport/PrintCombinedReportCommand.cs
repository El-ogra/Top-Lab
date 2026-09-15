using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;

public sealed record PrintCombinedReportCommand(
    int PatientId,
    IReadOnlyList<int> OrderedPatientTestIds) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ReportProductionAccessPolicy.PrintResults;
}
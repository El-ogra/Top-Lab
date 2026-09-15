using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;

public sealed record BuildCombinedReportCommand(
    int PatientId,
    IReadOnlyList<int> OrderedPatientTestIds) : IRequest<Result<CombinedReportDto>>;
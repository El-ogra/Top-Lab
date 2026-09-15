using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;

public sealed record InsertHistoryResultCommand(int PatientTestId, int SourcePatientTestId)
    : IRequest<Result<CombinedReportDto>>;
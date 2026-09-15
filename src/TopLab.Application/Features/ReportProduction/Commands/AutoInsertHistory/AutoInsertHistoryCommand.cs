using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.AutoInsertHistory;

public sealed record AutoInsertHistoryCommand(int PatientTestId)
    : IRequest<Result<CombinedReportDto>>;
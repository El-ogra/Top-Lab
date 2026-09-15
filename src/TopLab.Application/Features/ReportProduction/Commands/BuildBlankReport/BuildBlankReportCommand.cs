using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;

public sealed record BuildBlankReportCommand(int PatientId) : IRequest<Result<BlankReportDto>>;
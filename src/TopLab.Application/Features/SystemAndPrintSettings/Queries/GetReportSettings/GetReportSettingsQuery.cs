using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;

public sealed record GetReportSettingsQuery : IRequest<Result<ReportSettingsDto>>;
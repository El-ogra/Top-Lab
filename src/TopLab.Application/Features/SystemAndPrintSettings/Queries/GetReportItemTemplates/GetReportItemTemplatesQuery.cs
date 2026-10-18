using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportItemTemplates;

/// <summary>GAP-014 (Phase 7): reads the editable report-item template rows.</summary>
public sealed record GetReportItemTemplatesQuery : IRequest<Result<IReadOnlyList<ReportItemTemplateDto>>>;

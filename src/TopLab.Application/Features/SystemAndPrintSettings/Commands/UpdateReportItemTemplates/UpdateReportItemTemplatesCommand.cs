using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;

/// <summary>GAP-014 (Phase 7): persists the editable report-item template rows.</summary>
public sealed record UpdateReportItemTemplatesCommand(
    IReadOnlyList<ReportItemTemplateDto> Items)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}

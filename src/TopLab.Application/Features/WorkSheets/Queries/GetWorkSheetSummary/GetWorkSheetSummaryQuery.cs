using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;

public sealed record GetWorkSheetSummaryQuery(
    DateOnly? From = null,
    DateOnly? To = null)
    : IRequest<Result<IReadOnlyList<WorkSheetSummaryRowDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

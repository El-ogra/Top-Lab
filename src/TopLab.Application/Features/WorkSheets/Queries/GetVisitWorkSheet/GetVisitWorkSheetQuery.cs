using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;

/// <summary>
/// Per-visit worksheet for one patient (S-01 slice S4). Gated on
/// <c>PRINT_WORKSHEET</c>, consistent with the four existing M-11 queries.
/// </summary>
public sealed record GetVisitWorkSheetQuery(
    int PatientId)
    : IRequest<Result<VisitWorkSheetDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;

/// <summary>
/// Prints the per-visit worksheet for a patient. Gated on
/// <c>PRINT_WORKSHEET</c>, consistent with the M-11 worksheet queries.
/// </summary>
public sealed record PrintWorkSheetCommand(int PatientId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

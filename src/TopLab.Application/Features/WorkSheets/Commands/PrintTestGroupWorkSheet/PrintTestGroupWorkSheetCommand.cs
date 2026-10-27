using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Commands.PrintTestGroupWorkSheet;

/// <summary>
/// GAP-17 (2.1.3.5 / 2.3.1.5.1 / 2.3.1.5.3): prints the test-group worksheet.
/// The unfinished-only restriction is forwarded verbatim to the existing
/// <c>GetWorkSheetByTestGroupQuery</c>, whose predicate is
/// <c>PatientTest.EnteredAtUtc == null</c> (owner decision, 2026-10-26).
/// Gated on <c>PRINT_WORKSHEET</c>, consistent with the M-11 worksheet queries.
/// </summary>
public sealed record PrintTestGroupWorkSheetCommand(
    int TestGroupId,
    DateOnly? From = null,
    DateOnly? To = null,
    bool? UnfinishedOnly = null)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

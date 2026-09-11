using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;

public sealed record GetWorkSheetByTestGroupQuery(
    int? TestGroupId = null,
    IReadOnlyList<int>? TestIds = null,
    DateOnly? From = null,
    DateOnly? To = null)
    : IRequest<Result<WorkSheetDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;

public sealed record GetWorkSheetByWorkGroupLogQuery(
    int WorkGroupLogId,
    DateOnly? From = null,
    DateOnly? To = null)
    : IRequest<Result<WorkSheetDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

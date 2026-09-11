using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;

public sealed record GetWorkSheetTestCountByPeriodQuery(
    DateOnly? From = null,
    DateOnly? To = null)
    : IRequest<Result<WorkSheetTestCountDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => WorkSheetsAccessPolicy.PrintWorksheet;
}

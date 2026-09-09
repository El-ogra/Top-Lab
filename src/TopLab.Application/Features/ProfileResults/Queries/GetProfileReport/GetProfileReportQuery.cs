using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;

public sealed record GetProfileReportQuery(
    int PatientTestId)
    : IRequest<Result<ProfileReportDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.EditResults;
}
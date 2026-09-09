using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;

public sealed record GetProfileEntryGridQuery(
    int PatientTestId)
    : IRequest<Result<ProfileEntryGridDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.EditResults;
}
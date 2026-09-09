using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;

public sealed record UnverifyProfileResultsCommand(
    int PatientTestId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.ReviewResults;
}
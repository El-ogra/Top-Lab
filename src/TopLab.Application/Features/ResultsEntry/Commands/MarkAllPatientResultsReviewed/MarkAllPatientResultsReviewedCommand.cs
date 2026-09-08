using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;

public sealed record MarkAllPatientResultsReviewedCommand(int PatientId) : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.ReviewResults;
}

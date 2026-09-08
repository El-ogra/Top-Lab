using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;

namespace TopLab.Application.Features.SampleCollection.Commands.MarkSampleDrawn;

public sealed record MarkSampleDrawnCommand(
    int PatientTestId,
    DateTime DrawnAtUtc)
    : IRequest<Result<bool>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SampleCollectionAccessPolicy.AddEditPatient;
}

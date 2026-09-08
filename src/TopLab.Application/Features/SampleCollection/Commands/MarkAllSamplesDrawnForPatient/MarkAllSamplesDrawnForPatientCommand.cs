using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;

namespace TopLab.Application.Features.SampleCollection.Commands.MarkAllSamplesDrawnForPatient;

public sealed record MarkAllSamplesDrawnForPatientCommand(
    int PatientId)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SampleCollectionAccessPolicy.AddEditPatient;
}

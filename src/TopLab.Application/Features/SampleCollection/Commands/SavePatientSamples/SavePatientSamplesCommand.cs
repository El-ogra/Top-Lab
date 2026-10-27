using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;

namespace TopLab.Application.Features.SampleCollection.Commands.SavePatientSamples;

public sealed record SavePatientSamplesCommand(int PatientId, IReadOnlyList<int> DrawnPatientTestIds)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SampleCollectionAccessPolicy.AddEditPatient;
}
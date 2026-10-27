using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;

namespace TopLab.Application.Features.SampleCollection.Commands.PrintSampleStickers;

public sealed record PrintSampleStickersCommand(int PatientId, SampleStickerScope Scope)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SampleCollectionAccessPolicy.AddEditPatient;
}
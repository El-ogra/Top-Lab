using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SamplePipeline.Commands.EchoName;

/// <summary>
/// Sample use case used to exercise the MediatR pipeline behaviors in tests.
/// Requires a permission so the AuthorizationBehavior path can be asserted.
/// </summary>
public sealed record EchoNameCommand(string Name)
    : IRequest<Result<string>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "SAMPLE_PIPELINE";
}

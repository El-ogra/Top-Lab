using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.SamplePipeline.Commands.EchoName;

/// <summary>
/// Handler for <see cref="EchoNameCommand"/>. Assumes input is already valid
/// (ValidationBehavior runs first) and the caller is authorized (AuthorizationBehavior runs first).
/// </summary>
public sealed class EchoNameCommandHandler : IRequestHandler<EchoNameCommand, Result<string>>
{
    public Task<Result<string>> Handle(EchoNameCommand request, CancellationToken cancellationToken)
        => Task.FromResult(Result<string>.Success($"Hello {request.Name}"));
}

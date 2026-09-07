using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AccessAndNavigation.Commands.LockWorkstation;

/// <summary>
/// Handles <see cref="LockWorkstationCommand"/> by clearing the in-memory
/// current session. Purely in-memory: no <see cref="IApplicationDbContext"/>
/// is injected, mirroring the M-17 <c>SignOutCommandHandler</c> precedent.
/// Authorization is enforced upstream by <c>AuthorizationBehavior</c> based
/// on the command's <see cref="Common.Authorization.IAuthorizedRequest"/>.
/// </summary>
public sealed class LockWorkstationCommandHandler
    : IRequestHandler<LockWorkstationCommand, Result>
{
    private readonly ICurrentUserService _currentUser;

    public LockWorkstationCommandHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<Result> Handle(LockWorkstationCommand request, CancellationToken cancellationToken)
    {
        _currentUser.ClearSession();
        return Task.FromResult(Result.Success());
    }
}
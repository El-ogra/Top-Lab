using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SignOut;

public sealed class SignOutCommandHandler : IRequestHandler<SignOutCommand, Result>
{
    private readonly ICurrentUserService _currentUser;

    public SignOutCommandHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<Result> Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        _currentUser.ClearSession();
        return Task.FromResult(Result.Success());
    }
}

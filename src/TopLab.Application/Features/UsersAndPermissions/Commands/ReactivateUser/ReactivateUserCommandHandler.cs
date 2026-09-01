using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;

public sealed class ReactivateUserCommandHandler : IRequestHandler<ReactivateUserCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ReactivateUserCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == request.UserId);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("المستخدم غير موجود"));
        }

        user.Reactivate();
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

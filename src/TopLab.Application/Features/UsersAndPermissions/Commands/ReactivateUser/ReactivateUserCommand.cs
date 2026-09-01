using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;

public sealed record ReactivateUserCommand(int UserId) : IRequest<Result>;

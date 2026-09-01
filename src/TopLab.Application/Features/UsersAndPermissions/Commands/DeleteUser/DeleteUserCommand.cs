using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;

public sealed record DeleteUserCommand(int UserId) : IRequest<Result>;

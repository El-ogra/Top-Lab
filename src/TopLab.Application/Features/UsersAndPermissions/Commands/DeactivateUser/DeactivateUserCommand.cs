using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(int UserId) : IRequest<Result>;

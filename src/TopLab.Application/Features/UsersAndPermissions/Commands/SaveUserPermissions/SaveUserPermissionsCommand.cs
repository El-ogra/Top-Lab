using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SaveUserPermissions;

public sealed record SaveUserPermissionsCommand(int UserId, IReadOnlyList<string> PermissionCodes) : IRequest<Result>;

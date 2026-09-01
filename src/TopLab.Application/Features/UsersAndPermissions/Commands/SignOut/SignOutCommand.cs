using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SignOut;

public sealed record SignOutCommand : IRequest<Result>;

using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;

public sealed record SignInCommand(string UserName, string Password) : IRequest<Result<CurrentUserSessionDto>>;

using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.GetCurrentSession;

public sealed record GetCurrentSessionQuery : IRequest<Result<CurrentUserSessionDto>>;

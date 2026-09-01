using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.GetUserById;

public sealed record GetUserByIdQuery(int UserId) : IRequest<Result<UserDetailDto>>;

using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Common;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<Result<IReadOnlyList<UserSummaryDto>>>;

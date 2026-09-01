using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.HasAnyAbsoluteUser;

public sealed record HasAnyAbsoluteUserQuery : IRequest<Result<bool>>;

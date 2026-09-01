using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.UsersAndPermissions.Queries.VerifySecondaryPassword;

public sealed record VerifySecondaryPasswordQuery(string Password) : IRequest<Result<bool>>;

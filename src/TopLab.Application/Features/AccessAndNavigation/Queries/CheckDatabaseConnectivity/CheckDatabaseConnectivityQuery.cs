using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;

public sealed record CheckDatabaseConnectivityQuery : IRequest<Result<bool>>;

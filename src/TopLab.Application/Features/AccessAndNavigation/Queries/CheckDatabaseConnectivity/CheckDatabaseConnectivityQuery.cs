using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AccessAndNavigation.Common;

namespace TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;

public sealed record CheckDatabaseConnectivityQuery
    : IRequest<Result<CheckDatabaseConnectivityDto>>;
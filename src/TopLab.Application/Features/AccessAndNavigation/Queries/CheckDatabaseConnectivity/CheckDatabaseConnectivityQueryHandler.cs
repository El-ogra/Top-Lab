using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AccessAndNavigation.Common;
using TopLab.Application.Features.AccessAndNavigation.Common.Interfaces;

namespace TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;

/// <summary>
/// Returns a <see cref="CheckDatabaseConnectivityDto"/> describing whether
/// the SQL Server database is reachable together with the redacted server
/// and database name parsed from the workstation-local connection string.
/// Exceptions thrown by the database call are swallowed: the caller
/// always receives a successful DTO with <c>IsConnected = false</c> so a
/// UI status indicator never has to handle an exception path.
/// </summary>
public sealed class CheckDatabaseConnectivityQueryHandler
    : IRequestHandler<CheckDatabaseConnectivityQuery, Result<CheckDatabaseConnectivityDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDbConnectionDescriptor _descriptor;
    private readonly IDateTimeProvider _dateTime;

    public CheckDatabaseConnectivityQueryHandler(
        IApplicationDbContext db,
        IDbConnectionDescriptor descriptor,
        IDateTimeProvider dateTime)
    {
        _db = db;
        _descriptor = descriptor;
        _dateTime = dateTime;
    }

    public async Task<Result<CheckDatabaseConnectivityDto>> Handle(
        CheckDatabaseConnectivityQuery request,
        CancellationToken cancellationToken)
    {
        var serverName = _descriptor.ServerName;
        var databaseName = _descriptor.DatabaseName;

        try
        {
            var can = await _db.CanConnectAsync(cancellationToken);
            return Result<CheckDatabaseConnectivityDto>.Success(
                new CheckDatabaseConnectivityDto(can, serverName, databaseName, _dateTime.UtcNow));
        }
        catch (Exception)
        {
            return Result<CheckDatabaseConnectivityDto>.Success(
                new CheckDatabaseConnectivityDto(false, serverName, databaseName, _dateTime.UtcNow));
        }
    }
}
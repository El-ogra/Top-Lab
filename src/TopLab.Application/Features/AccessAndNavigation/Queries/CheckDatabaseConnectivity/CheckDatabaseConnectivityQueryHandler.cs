using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;

public sealed class CheckDatabaseConnectivityQueryHandler : IRequestHandler<CheckDatabaseConnectivityQuery, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public CheckDatabaseConnectivityQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(CheckDatabaseConnectivityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var can = await _db.CanConnectAsync(cancellationToken);
            return Result<bool>.Success(can);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.Unexpected(ex.Message));
        }
    }
}

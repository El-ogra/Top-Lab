using Microsoft.EntityFrameworkCore;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Infrastructure.Persistence;

/// <summary>
/// W-02 S14 (WP-29): EF transaction behind <see cref="IAppUnitOfWork"/>.
/// Lives in Infrastructure so Application never sees a <c>DbContext</c>.
/// </summary>
public sealed class AppUnitOfWork : IAppUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public AppUnitOfWork(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> work, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(work, System.Data.IsolationLevel.ReadCommitted, cancellationToken);
    }

    public Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> work,
        System.Data.IsolationLevel isolation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(isolation, cancellationToken);
            try
            {
                var result = await work(cancellationToken);
                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}

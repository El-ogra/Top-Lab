using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>W-02 S14 (WP-29): inline unit of work — records the requested isolation
/// level so tests can prove the settlement path asks for a serial lock.</summary>
public sealed class FakeAppUnitOfWork : IAppUnitOfWork
{
    public List<System.Data.IsolationLevel> RequestedIsolations { get; } = new();

    public int Executions { get; private set; }

    public Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> work, CancellationToken cancellationToken = default)
    {
        Executions++;
        return work(cancellationToken);
    }

    public Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> work,
        System.Data.IsolationLevel isolation,
        CancellationToken cancellationToken = default)
    {
        RequestedIsolations.Add(isolation);
        Executions++;
        return work(cancellationToken);
    }
}

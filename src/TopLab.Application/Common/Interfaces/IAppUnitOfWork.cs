using TopLab.Application.Common.Results;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// W-02 S14 (WP-29): unit-of-work boundary for multi-step visit edits. The work
/// runs inside one transaction; a failure result rolls everything back.
/// The interface never exposes a <c>DbContext</c> (VG-14 structural pin).
/// </summary>
public interface IAppUnitOfWork
{
    Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> work, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same boundary under an explicit isolation level. W-02 S14 uses
    /// <see cref="System.Data.IsolationLevel.Serializable"/> for settlement so two
    /// concurrent settlements serialise instead of double-inserting — without any
    /// raw SQL in Application (the stage plan's UPDLOCK sketch would have been the
    /// first <c>FromSql</c> in the codebase; <c>IApplicationDbContext</c> exposes no
    /// <c>Database</c> facade, so isolation is expressed here, on the new port).
    /// </summary>
    Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> work, System.Data.IsolationLevel isolation, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Common;

namespace TopLab.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that populates the audit columns
/// of every <see cref="AuditableEntity{TId}"/> attached to the change tracker
/// (ADR-0013, Architecture §6.4, Coding Standards §6.4). Handlers never set these
/// fields manually; this is the single writer of audit data.
/// </summary>
/// <remarks>
/// On <c>Added</c> the interceptor sets <c>CreatedByUserId</c>, <c>CreatedAtUtc</c>,
/// <c>LastModifiedByUserId</c>, <c>LastModifiedAtUtc</c> and
/// <c>ModificationCount = 0</c>. On <c>Modified</c> it updates the last-modified
/// fields, increments <c>ModificationCount</c>, and leaves the created fields
/// untouched. The <c>IDateTimeProvider</c> is resolved from the current scope so
/// tests can substitute a deterministic clock; the current-user service is
/// resolved the same way to allow authenticated tests to assert audit columns.
/// </remarks>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUser, IDateTimeProvider dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditData(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditData(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditData(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var nowUtc = _dateTime.UtcNow;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : 0;

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not AuditableEntity<int> auditable)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    auditable.CreatedByUserId = userId;
                    auditable.CreatedAtUtc = nowUtc;
                    auditable.LastModifiedByUserId = userId;
                    auditable.LastModifiedAtUtc = nowUtc;
                    auditable.ModificationCount = 0;
                    break;

                case EntityState.Modified:
                    // Audit fields are owned by this interceptor; any handler-side
                    // changes to them are reverted so the persisted value is the
                    // one we just computed.
                    entry.Property(nameof(AuditableEntity<int>.CreatedByUserId)).IsModified = false;
                    entry.Property(nameof(AuditableEntity<int>.CreatedAtUtc)).IsModified = false;
                    auditable.LastModifiedByUserId = userId;
                    auditable.LastModifiedAtUtc = nowUtc;
                    auditable.ModificationCount++;
                    break;
            }
        }
    }
}

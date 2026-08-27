using System;

namespace TopLab.Domain.Common;

/// <summary>
/// Base class for entities that require creation/modification tracking.
/// The audit fields are populated automatically at persistence time by
/// <c>AuditableEntitySaveChangesInterceptor</c> in the Infrastructure layer
/// (ADR-0013) — domain/handler code must never set them manually.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    public int CreatedByUserId { get; internal set; }

    public DateTime CreatedAtUtc { get; internal set; }

    public int LastModifiedByUserId { get; internal set; }

    public DateTime LastModifiedAtUtc { get; internal set; }

    public int ModificationCount { get; internal set; }

    protected AuditableEntity()
    {
    }

    protected AuditableEntity(TId id)
        : base(id)
    {
    }
}

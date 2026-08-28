using System;

namespace TopLab.Domain.Common;

/// <summary>
/// Base class for entities that require creation/modification tracking.
/// The audit fields are populated automatically at persistence time by
/// <c>AuditableEntitySaveChangesInterceptor</c> in the Infrastructure layer
/// (ADR-0013) — domain/handler code must never set them manually.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    where TId : notnull
{
    public int CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int LastModifiedByUserId { get; set; }

    public DateTime LastModifiedAtUtc { get; set; }

    public int ModificationCount { get; set; }

    protected AuditableEntity()
    {
    }

    protected AuditableEntity(TId id)
        : base(id)
    {
    }
}

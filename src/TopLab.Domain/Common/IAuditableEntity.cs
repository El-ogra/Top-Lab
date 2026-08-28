namespace TopLab.Domain.Common;

/// <summary>
/// Marker for entities carrying audit columns. Allows the save-changes
/// interceptor to handle any <c>AuditableEntity&lt;TId&gt;</c> regardless
/// of its <c>TId</c> (ADR-0013, F5 fix).
/// </summary>
public interface IAuditableEntity
{
    int CreatedByUserId { get; set; }

    DateTime CreatedAtUtc { get; set; }

    int LastModifiedByUserId { get; set; }

    DateTime LastModifiedAtUtc { get; set; }

    int ModificationCount { get; set; }
}

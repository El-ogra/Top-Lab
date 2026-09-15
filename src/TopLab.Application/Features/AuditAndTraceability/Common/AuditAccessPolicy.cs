namespace TopLab.Application.Features.AuditAndTraceability.Common;

/// <summary>
/// Permission codes consumed by the M-10 audit &amp; traceability queries.
/// Both the P view and the T view are gated on PT_AUDIT_ACCESS (seeded id=13,
/// FR-M17-008); absolute-permission users bypass via the existing pipeline.
/// </summary>
public static class AuditAccessPolicy
{
    public const string PtAuditAccess = "PT_AUDIT_ACCESS";
}

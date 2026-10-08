namespace TopLab.Application.Features.PatientEnvelope.Common;

/// <summary>
/// Live envelope content for one patient visit (Phase 1, REF-066).
/// Computed at print time from existing rows — no stored state, no migration.
/// </summary>
public sealed record EnvelopeDto(
    int PatientId,
    string FullName,
    string Identifier,
    string? ReferralDisplayName,
    DateTime DateUtc,
    string? LabId);
